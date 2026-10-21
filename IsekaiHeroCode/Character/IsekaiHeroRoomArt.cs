using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace IsekaiHero.IsekaiHeroCode.Character;

/// <summary>Replace the inherited Spine artwork while retaining the game's room controls.</summary>
internal static class IsekaiHeroRoomArt
{
    public const string RestTexturePath = "res://IsekaiHero/images/character/hero_rest_site_v1.png";
    public const string ShopTexturePath = "res://IsekaiHero/images/character/hero_shop_v1.png";
    internal const string VisualName = "IsekaiHeroRoomVisuals";

    public static void Install(Node2D root, bool rest)
    {
        var parent = rest ? root.GetNode("ControlRoot") : root;
        if (parent.HasNode(VisualName)) return;
        var texture = GD.Load<Texture2D>(rest ? RestTexturePath : ShopTexturePath);
        if (texture == null) return;

        // Keep the native scene and its animation calls alive. Only its art is hidden.
        // Rest-site targeting, thought bubbles, selection and room layout stay native.
        foreach (var child in root.GetChildren())
            if (child is CanvasItem item && child.GetClass() == "SpineSprite")
                item.Visible = false;

        var body = new Node2D { Name = VisualName };
        var factor = rest ? 0.56f : 0.30f;
        body.AddChild(new Sprite2D
        {
            Name = "Sprite",
            Texture = texture,
            Centered = false,
            Scale = Vector2.One * factor,
            Position = rest
                ? new Vector2(-texture.GetWidth() * factor / 2 + 55, -364)
                : new Vector2(-texture.GetWidth() * factor / 2, -453)
        });
        // ControlRoot is mirrored by the game's FlipX, including the sprite.
        parent.AddChild(body);
        parent.MoveChild(body, 0);
        if (rest)
        {
            var bounds = new Rect2(-200, -335, 530, 660);
            foreach (var path in new[] { "%Hitbox", "%SelectionReticle" })
            {
                var control = root.GetNode<Control>(path);
                control.Position = bounds.Position;
                control.Size = bounds.Size;
            }
        }
        var breathing = body.CreateTween().SetLoops();
        breathing.TweenProperty(body, "scale", new Vector2(1.003f, 1.007f), 1.8)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        breathing.TweenProperty(body, "scale", Vector2.One, 1.8)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }
}

[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
internal static class IsekaiHeroRestSiteArtPatch
{
    [HarmonyPostfix]
    private static void Postfix(NRestSiteCharacter __instance)
    {
        if (__instance.Player.Character is IsekaiHero)
            IsekaiHeroRoomArt.Install(__instance, rest: true);
    }
}

[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
internal static class IsekaiHeroShopArtPatch
{
    [HarmonyPostfix]
    private static void Postfix(NMerchantRoom __instance, List<Player> ____players)
    {
        // The room sorts the local player first, then builds PlayerVisuals in this order.
        for (var index = 0; index < Math.Min(____players.Count, __instance.PlayerVisuals.Count); index++)
            if (____players[index].Character is IsekaiHero)
                IsekaiHeroRoomArt.Install(__instance.PlayerVisuals[index], rest: false);
    }
}

[HarmonyPatch(typeof(NMerchantCharacter), nameof(NMerchantCharacter.PlayAnimation))]
internal static class IsekaiHeroShopDeathArtPatch
{
    [HarmonyPostfix]
    private static void Postfix(NMerchantCharacter __instance, string anim)
    {
        if (anim != "die") return;
        var body = __instance.GetNodeOrNull<Node2D>(IsekaiHeroRoomArt.VisualName);
        if (body == null) return;
        var slump = body.CreateTween().SetParallel();
        slump.TweenProperty(body, "rotation", -0.12f, 0.5);
        slump.TweenProperty(body, "modulate", new Color(0.65f, 0.65f, 0.65f), 0.5);
    }
}
