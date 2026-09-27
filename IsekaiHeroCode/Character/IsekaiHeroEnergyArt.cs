using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace IsekaiHero.IsekaiHeroCode.Character;

[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter.Create))]
internal static class IsekaiHeroEnergyArt
{
    private static ImageTexture? _filteredTexture;
    private static Shader? _shader;

    [HarmonyPostfix]
    private static void Postfix(Player player, NEnergyCounter? __result)
    {
        if (player.Character is not IsekaiHero || __result == null) return;
        var art = __result.GetNode<TextureRect>("Layers/Layer1");
        if (_filteredTexture == null)
        {
            using var image = art.Texture.GetImage();
            // The quick packer does not supply mipmaps. Generate the full chain
            // once, so a 1254px painting stays smooth at the 128px HUD size.
            image.GenerateMipmaps();
            _filteredTexture = ImageTexture.CreateFromImage(image);
        }
        art.Texture = _filteredTexture;
        art.TextureFilter = CanvasItem.TextureFilterEnum.LinearWithMipmaps;
        _shader ??= new Shader { Code = ShaderCode };
        art.Material = new ShaderMaterial { Shader = _shader };
        art.SetMeta("isekai_energy_material", art.Material);
    }

    // RefreshLabel replaces layer materials when energy changes. Restore ours
    // afterwards, preserving the game's depleted modulation and number label.
    [HarmonyPatch(typeof(NEnergyCounter), "RefreshLabel")]
    private static class RefreshPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NEnergyCounter __instance)
        {
            var art = __instance.GetNodeOrNull<TextureRect>("Layers/Layer1");
            if (art == null || !art.HasMeta("isekai_energy_material")) return;
            var material = (ShaderMaterial)art.GetMeta("isekai_energy_material").AsGodotObject();
            material.SetShaderParameter("depleted", art.Material != null);
            art.Material = material;
        }
    }

    internal const string ShaderCode = """
        shader_type canvas_item;
        uniform bool depleted = false;
        varying vec4 tint;

        void vertex() {
            tint = COLOR;
        }

        void fragment() {
            vec2 center = vec2(0.5, 0.5);
            vec2 delta = UV - center;
            float angle = TIME * 0.523598776;
            float c = cos(angle);
            float s = sin(angle);
            vec2 rotated = center + mat2(vec2(c, s), vec2(-s, c)) * delta;
            // Keep all crystal facets stationary. Blend only inside the round
            // painted well, well clear of the diamond's inner rim.
            float mask = 1.0 - smoothstep(0.205, 0.235, length(delta));
            vec4 ink = mix(texture(TEXTURE, UV), texture(TEXTURE, rotated), mask);
            if (depleted) {
                float gray = dot(ink.rgb, vec3(0.299, 0.587, 0.114));
                ink.rgb = mix(ink.rgb, vec3(gray), 0.75);
            }
            COLOR = ink * tint;
        }
        """;
}
