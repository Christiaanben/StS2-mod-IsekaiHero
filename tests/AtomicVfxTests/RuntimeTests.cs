using Godot;
using IsekaiHero.IsekaiHeroCode.Vfx;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

// Real scene-tree processing; no direct _Ready/_Process/_Draw calls or tween stepping.
public partial class RuntimeTests : Node
{
    public override async void _Ready()
    {
        try
        {
            if (typeof(GodotObject).IsAssignableFrom(typeof(AtomicVfx)))
                throw new Exception("Atomic must not use the custom C# script dispatcher that fails in the game.");

            var room = new NCombatRoom(); NCombatRoom.Instance = room;
            AddChild(room.CombatVfxContainer);
            foreach (var (mode, replay, reduced) in new[]
            {
                (FastModeType.Normal, false, false), (FastModeType.Fast, false, false),
                (FastModeType.Instant, false, false), (FastModeType.Normal, true, false),
                (FastModeType.Normal, false, true)
            })
            {
                var prefs = SaveManager.Instance.PrefsSave;
                prefs.FastMode = mode; prefs.TextEffectsEnabled = !reduced;
                var effect = await AtomicVfx.Charge(new(), replay) ?? throw new Exception("Charge was not created.");
                var root = room.CombatVfxContainer.GetNode<Node2D>("IAmAtomicVfx");
                CheckNativeTree(root);
                if (mode != FastModeType.Instant && root.GetChild<Node2D>(0).Scale.X >= 1)
                    throw new Exception("Native charge tween did not run.");
                effect.Detonate();
                if (!root.GetChild<Node2D>(1).Visible || root.GetChild<Node2D>(0).Visible)
                    throw new Exception("Detonation did not replace the charge.");
                var children = root.GetChildCount();
                if (reduced && children != 19) throw new Exception("Reduced effects contains the bright core.");
                await Wait(.08);
                if (root.GetChild<Node2D>(1).Scale.X <= 1)
                    throw new Exception("Native detonation tween did not run.");
                await Wait(.7);
                if (GodotObject.IsInstanceValid(root)) throw new Exception("Native tween did not free the effect.");
                GD.Print($"PASS Atomic {mode}, replay={replay}, reduced={reduced}: automatic charge, detonation, cleanup");
            }
            var interrupted = await AtomicVfx.Charge(new(), false) ?? throw new Exception("No interrupted charge.");
            var interruptedRoot = room.CombatVfxContainer.GetNode<Node2D>("IAmAtomicVfx");
            await Wait(3.2);
            if (GodotObject.IsInstanceValid(interruptedRoot)) throw new Exception("Interrupted charge leaked.");
            interrupted.Detonate(); // Cleanup racing with damage must be harmless.
            GD.Print("PASS Atomic interrupted-play cleanup");
            GD.Print("6/6 engine checks passed.");
            GetTree().Quit();
        }
        catch (Exception error) { GD.PrintErr(error); GetTree().Quit(1); }
    }

    private static void CheckNativeTree(Node node)
    {
        if (node.GetScript().VariantType != Variant.Type.Nil)
            throw new Exception("Effect contains a script callback dependency.");
        foreach (var child in node.GetChildren()) CheckNativeTree(child);
    }

    private async Task Wait(double seconds) => await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
}
