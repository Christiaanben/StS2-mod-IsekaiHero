using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace IsekaiHero.IsekaiHeroCode.Vfx;

// Entirely local presentation: no gameplay RNG, camera transform, or global audio state.
public partial class AtomicVfx : Node2D
{
    private static readonly Color Violet = new("a94bff");
    private static readonly Color Cyan = new("9aeaff");
    private float _age;
    private float _charge = .75f;
    private float _afterglow = .65f;
    private float _reach = 1300;
    private bool _impact;
    private bool _reduced;
    private Label? _declaration;

    public static async Task<AtomicVfx?> Charge(Creature caster, bool replay)
    {
        var room = NCombatRoom.Instance;
        var creatureNode = room?.GetCreatureNode(caster);
        if (room == null || creatureNode == null) return null;

        var prefs = SaveManager.Instance.PrefsSave;
        var compact = replay || prefs.FastMode is FastModeType.Fast or FastModeType.Instant;
        var effect = new AtomicVfx
        {
            _charge = compact ? .2f : .75f,
            _afterglow = compact ? .25f : .65f,
            _reduced = !prefs.TextEffectsEnabled || prefs.ScreenShakeOptionIndex == 0,
            ZIndex = 5
        };
        room.CombatVfxContainer.AddChild(effect);
        effect.GlobalPosition = creatureNode.VfxSpawnPosition;
        effect._reach = Math.Max(900, effect.GetViewportRect().Size.X / effect.GlobalScale.X);
        await Cmd.Wait(effect._charge);
        return GodotObject.IsInstanceValid(effect) && effect.IsInsideTree() ? effect : null;
    }

    public override void _Ready()
    {
        _declaration = new Label
        {
            Position = new Vector2(-185, -190), Size = new Vector2(370, 90),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Modulate = new Color(1, .91f, 1)
        };
        _declaration.AddThemeFontSizeOverride("font_size", 42);
        _declaration.AddThemeColorOverride("font_outline_color", new Color("180e2b"));
        _declaration.AddThemeConstantOverride("outline_size", 8);
        AddChild(_declaration);
    }

    public void Detonate()
    {
        _impact = true;
        _age = 0;
        if (_declaration != null) _declaration.Text = "ATOMIC";
        if (!_reduced)
            NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        _age += (float)delta;
        if (_impact && _age >= _afterglow || !_impact && _age > _charge + 3)
        {
            QueueFree();
            return;
        }
        if (_declaration != null)
        {
            if (!_impact)
                _declaration.Text = _age < _charge * .42f ? "" : _age < _charge * .7f ? "I" : "I AM";
            else
                _declaration.Modulate = new Color(1, .91f, 1, 1 - _age / _afterglow);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_impact)
        {
            var t = Mathf.Clamp(_age / _charge, 0, 1);
            if (!_reduced)
                DrawRect(new Rect2(-_reach, -_reach, _reach * 2, _reach * 2), new Color(.025f, .01f, .05f, .16f * t));
            DrawArc(Vector2.Zero, 70 + 25 * t, -Mathf.Pi, Mathf.Pi, 12, Violet, 5, true);
            DrawArc(Vector2.Zero, 100 + 10 * t, -Mathf.Pi, Mathf.Pi, 6, Cyan, 2, true);
            // Deterministic motes contract; presentation never advances combat RNG.
            for (var i = 0; i < 12; i++)
            {
                var direction = Vector2.FromAngle(i * Mathf.Tau / 12 + .3f);
                var center = direction * (180 - 130 * t);
                DrawShard(center, direction, 7, Violet);
            }
            DrawLine(new Vector2(0, -240), new Vector2(0, 130), new Color(.8f, .6f, 1, t), 2 + t * 3, true);
            return;
        }

        var progress = Mathf.Clamp(_age / _afterglow, 0, 1);
        var alpha = 1 - progress;
        var radius = 60 + _reach * Mathf.Ease(progress, .4f);
        DrawArc(Vector2.Zero, radius + 12, -Mathf.Pi, Mathf.Pi, 48, new Color(.12f, .04f, .2f, alpha), 24, true);
        DrawArc(Vector2.Zero, radius, -Mathf.Pi, Mathf.Pi, 48, new Color(Violet, alpha), 12, true);
        DrawArc(Vector2.Zero, radius - 8, -Mathf.Pi, Mathf.Pi, 48, new Color(Cyan, alpha), 3, true);
        if (!_reduced)
        {
            var core = Mathf.Max(0, 1 - progress * 3);
            DrawShard(Vector2.Zero, Vector2.Up, 90 * core, new Color(1, .9f, 1, core));
            DrawLine(new Vector2(0, -400), new Vector2(0, 250), new Color(Cyan, core), 10 * core + 1, true);
        }
        for (var i = 0; i < 16; i++)
        {
            var direction = Vector2.FromAngle(i * Mathf.Tau / 16 + .1f);
            if (i % 2 == 0)
            {
                var normal = direction.Orthogonal();
                var tip = direction * (radius + 100);
                DrawColoredPolygon([direction * (radius * .55f), direction * radius + normal * 15,
                    tip, direction * radius - normal * 5], new Color(Violet, alpha * .6f));
            }
            var center = direction * (80 + 160 * progress) + Vector2.Up * progress * 35;
            DrawShard(center, direction, 6 * alpha, new Color(Violet, alpha));
        }
    }

    private void DrawShard(Vector2 center, Vector2 direction, float size, Color color)
    {
        var normal = direction.Orthogonal();
        DrawColoredPolygon([center + direction * size * 2, center + normal * size,
            center - direction * size, center - normal * size], color);
    }
}
