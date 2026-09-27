using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace IsekaiHero.IsekaiHeroCode.Character;

/// <summary>Native Godot animation, dispatched by BaseLib's custom animation hooks.</summary>
internal static class IsekaiHeroVisuals
{
    public const string AtlasPath = "res://IsekaiHero/images/character/hero_combat_atlas.png";
    public const string AnimationPath = "res://IsekaiHero/animations/hero_combat.tres";

    public static NCreatureVisuals Create()
    {
        var root = new NCreatureVisuals { Name = "IsekaiHeroVisuals" };
        var body = Add(root, new Node2D { Name = "Visuals" });
        // Keep the feet at the origin. Per-pose atlas offsets live in the animation library.
        var sprite = new Sprite2D
        {
            Name = "Sprite",
            Texture = GD.Load<Texture2D>(AtlasPath),
            Centered = false,
            RegionEnabled = true,
            RegionFilterClipEnabled = true,
            RegionRect = new Rect2(0, 0, 512, 512),
            Offset = new Vector2(-285, -480),
            Scale = new Vector2(0.65f, 0.65f)
        };
        body.AddChild(sprite);
        sprite.Owner = root;

        Add(root, new Control
        {
            Name = "Bounds",
            Position = new Vector2(-105, -275),
            Size = new Vector2(210, 275),
            MouseFilter = Control.MouseFilterEnum.Ignore
        });
        Add(root, new Marker2D { Name = "IntentPos", Position = new Vector2(0, -310) });
        Add(root, new Marker2D { Name = "CenterPos", Position = new Vector2(0, -145) });
        Add(root, new Marker2D { Name = "OrbPos", Position = new Vector2(-100, -240) });
        Add(root, new Marker2D { Name = "TalkPos", Position = new Vector2(40, -280) });

        var player = Add(root, new AnimationPlayer { Name = "AnimationPlayer" });
        player.AddAnimationLibrary("", GD.Load<AnimationLibrary>(AnimationPath));
        foreach (var action in new[] { "Attack", "Cast", "Hit", "Revive" })
            player.AnimationSetNext(action, "Idle");
        // Dead deliberately holds its final pose. Relaxed and Idle loop.
        player.Autoplay = "Idle";
        return root;
    }

    private static T Add<T>(NCreatureVisuals root, T node) where T : Node
    {
        root.AddChild(node);
        node.Owner = root;
        node.UniqueNameInOwner = true;
        return node;
    }
}
