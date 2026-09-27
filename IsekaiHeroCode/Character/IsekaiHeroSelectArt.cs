using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace IsekaiHero.IsekaiHeroCode.Character;

// Replace only our placeholder background; the game's foreground UI remains intact.
[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
internal static class IsekaiHeroSelectArtPatch
{
    [HarmonyPostfix]
    private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
        => Install(__instance, characterModel);

    internal static void Install(Control screen, CharacterModel character)
    {
        if (character is not IsekaiHero) return;
        var background = screen.FindChild(character.Id.Entry + "_bg", true, false) as Control;
        if (background == null || background is TextureRect) return;
        // Replace the entire authored scene, including its oversized rectangle and
        // resize-driven zoom script. Keeping that root crops our full-screen cover.
        var parent = background.GetParent();
        var index = background.GetIndex();
        var name = background.Name;
        parent.RemoveChild(background);
        background.QueueFree();
        var art = IsekaiHeroSelectArt.Create();
        art.Name = name;
        parent.AddChild(art);
        parent.MoveChild(art, index);
        // AnimatedBg itself has oversized offsets and a 1.1 scale. Map the
        // screen rectangle into that parent's coordinates instead of filling it.
        var container = (Control)parent;
        void FitToScreen()
        {
            var transform = container.GetGlobalTransform().AffineInverse() * screen.GetGlobalTransform();
            art.Position = transform.Origin;
            art.Rotation = transform.Rotation;
            art.Scale = transform.Scale;
            art.Size = screen.Size;
        }
        screen.Resized += FitToScreen;
        container.ItemRectChanged += FitToScreen;
        art.TreeExiting += () =>
        {
            screen.Resized -= FitToScreen;
            container.ItemRectChanged -= FitToScreen;
        };
        FitToScreen();
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnLocalCharacterChangedForRandom")]
internal static class IsekaiHeroRandomSelectArtPatch
{
    [HarmonyPostfix]
    private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
        => IsekaiHeroSelectArtPatch.Install(__instance, characterModel);
}

internal static class IsekaiHeroSelectArt
{
    public const string TexturePath = "res://IsekaiHero/images/character/hero_select_cover_v3.png";

    public static Control Create()
    {
        var material = new ShaderMaterial
        {
            Shader = new Shader { Code = ShaderCode }
        };
        var art = new TextureRect
        {
            Name = "IsekaiCover",
            Texture = GD.Load<Texture2D>(TexturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = material
        };
        // This small target is behind the game's UI, and never consumes clicks.
        var hotspot = new Control
        {
            Name = "CrystalHover",
            MouseFilter = Control.MouseFilterEnum.Pass,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand
        };
        art.AddChild(hotspot);
        void LayoutHotspot()
        {
            var source = art.Texture.GetSize();
            var scale = Mathf.Max(art.Size.X / source.X, art.Size.Y / source.Y);
            var extent = source * scale;
            var center = (art.Size - extent) / 2 + extent * new Vector2(0.206f, 0.18f);
            var radius = extent.Y * 0.12f;
            hotspot.Position = center - Vector2.One * radius;
            hotspot.Size = Vector2.One * radius * 2;
        }
        art.Resized += LayoutHotspot;
        Tween? hoverTween = null;
        void SetHover(float value)
        {
            hoverTween?.Kill();
            hoverTween = art.CreateTween();
            hoverTween.TweenProperty(material, "shader_parameter/hover", value, 0.25);
        }
        hotspot.MouseEntered += () => SetHover(1);
        hotspot.MouseExited += () => SetHover(0);
        return art;
    }

    // Native rendering avoids managed Godot virtual callbacks, which failed in the game runtime.
    internal const string ShaderCode = """
        shader_type canvas_item;
        uniform float hover = 0.0;
        void fragment() {
            vec4 base = texture(TEXTURE, UV);
            vec2 metric = vec2(1.7777778, 1.0);
            float distance_to_crystal = length((UV - vec2(0.206, 0.18)) * metric);
            float pulse = 0.7 + 0.3 * sin(TIME * 2.0);
            float glow = exp(-distance_to_crystal * distance_to_crystal * 420.0);
            vec3 light = vec3(0.12, 0.65, 0.85) * glow * pulse * (0.07 + hover * 0.22);
            for (int i = 0; i < 24; i++) {
                float index = float(i);
                float phase = fract(index * 0.618034 + TIME * (0.018 + mod(index, 4.0) * 0.004));
                vec2 point = vec2(fract(index * 0.381966) + sin(TIME * 0.4 + index) * 0.008, 1.0 - phase);
                float d = length((UV - point) * metric);
                float mote = 1.0 - smoothstep(0.0005, 0.0025, d);
                light += vec3(0.3, 0.8, 1.0) * mote * sin(phase * 3.14159265) * 0.4;
            }
            COLOR = vec4(base.rgb + light, base.a);
        }
        """;
}
