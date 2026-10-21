using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace IsekaiHero.IsekaiHeroCode.Vfx;

// A managed controller, deliberately NOT a Godot script subclass. The shipped game
// rejects the SDK-generated InvokeGodotClassMethod dispatcher for custom C# nodes.
// Built-in nodes and native property tweens need no mod-defined Godot callbacks.
public sealed class AtomicVfx
{
    private static readonly Color Violet = new("a94bff");
    private static readonly Color Cyan = new("9aeaff");
    private readonly Node2D _root = new() { Name = "IAmAtomicVfx", ZIndex = 5 };
    private readonly Node2D _gather = new();
    private readonly Node2D _blast = new() { Visible = false };
    private readonly Label _declaration = new()
    {
        Position = new Vector2(-185, -190), Size = new Vector2(370, 90),
        HorizontalAlignment = HorizontalAlignment.Center,
        MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = new Color(1, .91f, 1)
    };
    private readonly float _charge;
    private readonly float _afterglow;
    private readonly bool _reduced;
    private readonly float _reach;
    internal Tween? Animation { get; private set; }

    private AtomicVfx(bool compact, bool reduced, float reach)
    {
        _charge = compact ? .2f : .75f;
        _afterglow = compact ? .25f : .65f;
        _reduced = reduced;
        _reach = reach;
        _declaration.AddThemeFontSizeOverride("font_size", 42);
        _declaration.AddThemeColorOverride("font_outline_color", new Color("180e2b"));
        _declaration.AddThemeConstantOverride("outline_size", 8);
        _root.AddChild(_gather);
        _root.AddChild(_blast);
        _root.AddChild(_declaration);
        AddRing(_gather, 85, 12, Violet, 5);
        AddRing(_gather, 110, 6, Cyan, 2);
        _gather.AddChild(new Line2D
        {
            Points = [new(0, -240), new(0, 130)], Width = 4, DefaultColor = Violet, Antialiased = true
        });
        for (var i = 0; i < 12; i++)
        {
            var direction = Vector2.FromAngle(i * Mathf.Tau / 12 + .3f);
            _gather.AddChild(Shard(direction * 170, direction, 7, Violet));
        }
        AddRing(_blast, 100, 48, new Color("27163d"), 18);
        AddRing(_blast, 98, 48, Violet, 9);
        AddRing(_blast, 95, 48, Cyan, 2);
        for (var i = 0; i < 8; i++)
        {
            var direction = Vector2.FromAngle(i * Mathf.Tau / 8 + .1f);
            var normal = direction.Orthogonal();
            _blast.AddChild(new Polygon2D
            {
                Polygon = [direction * 65, direction * 100 + normal * 3,
                    direction * 125, direction * 100 - normal * 1], Color = new Color(Violet, .6f)
            });
        }
    }

    public static async Task<AtomicVfx?> Charge(Creature caster, bool replay)
    {
        var room = NCombatRoom.Instance;
        var creatureNode = room?.GetCreatureNode(caster);
        if (room == null || creatureNode == null) return null;
        var prefs = SaveManager.Instance.PrefsSave;
        var compact = replay || prefs.FastMode is FastModeType.Fast or FastModeType.Instant;
        var effect = new AtomicVfx(compact,
            !prefs.TextEffectsEnabled || prefs.ScreenShakeOptionIndex == 0,
            Math.Max(900, room.CombatVfxContainer.GetViewportRect().Size.X));
        room.CombatVfxContainer.AddChild(effect._root);
        effect._root.GlobalPosition = creatureNode.VfxSpawnPosition;
        effect.StartGather();
        await Cmd.Wait(effect._charge);
        return GodotObject.IsInstanceValid(effect._root) && effect._root.IsInsideTree() ? effect : null;
    }

    private void StartGather()
    {
        Animation = _root.CreateTween().SetParallel();
        Animation.TweenProperty(_gather, "scale", Vector2.One * .65f, _charge);
        Animation.TweenProperty(_gather, "rotation", .3f, _charge);
        _declaration.Text = "I";
        Animation.TweenCallback(Callable.From(() => _declaration.Text = "I AM")).SetDelay(_charge * .55);
        // An interrupted play cannot leave a permanent effect in the room.
        Animation.Chain().TweenCallback(Callable.From(_root.QueueFree)).SetDelay(3);
    }

    public void Detonate()
    {
        if (!GodotObject.IsInstanceValid(_root) || !_root.IsInsideTree()) return;
        Animation?.Kill();
        _gather.Visible = false;
        _blast.Visible = true;
        _declaration.Text = "ATOMIC";
        Animation = _root.CreateTween().SetParallel();
        Animation.TweenProperty(_blast, "scale", Vector2.One * (_reach / 100), _afterglow)
            .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        Animation.TweenProperty(_blast, "modulate:a", 0f, _afterglow);
        Animation.TweenProperty(_declaration, "modulate:a", 0f, _afterglow);
        for (var i = 0; i < 16; i++)
        {
            var direction = Vector2.FromAngle(i * Mathf.Tau / 16 + .1f);
            var ash = Shard(direction * 80, direction, 6, Violet);
            _root.AddChild(ash);
            Animation.TweenProperty(ash, "position", direction * 240 + Vector2.Up * 35, _afterglow);
            Animation.TweenProperty(ash, "modulate:a", 0f, _afterglow);
        }
        if (!_reduced)
        {
            var core = Shard(Vector2.Zero, Vector2.Up, 90, new Color(1, .9f, 1));
            _root.AddChild(core);
            Animation.TweenProperty(core, "scale", Vector2.Zero, _afterglow / 3);
            NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
        }
        Animation.Chain().TweenCallback(Callable.From(_root.QueueFree));
    }

    private static void AddRing(Node2D parent, float radius, int segments, Color color, float width)
    {
        var points = new Vector2[segments + 1];
        for (var i = 0; i <= segments; i++)
            points[i] = Vector2.FromAngle(i * Mathf.Tau / segments) * radius;
        parent.AddChild(new Line2D { Points = points, Width = width, DefaultColor = color, Antialiased = true });
    }

    private static Polygon2D Shard(Vector2 center, Vector2 direction, float size, Color color)
    {
        var normal = direction.Orthogonal();
        return new Polygon2D
        {
            Position = center, Color = color,
            Polygon = [direction * size * 2, normal * size, -direction * size, -normal * size]
        };
    }
}
