using Godot;
namespace MegaCrit.Sts2.Core.Commands
{
    public static class Cmd
    {
        public static async Task Wait(float seconds)
        {
            if (Saves.SaveManager.Instance.PrefsSave.FastMode == Settings.FastModeType.Instant) return;
            var tree = (SceneTree)Engine.GetMainLoop();
            await tree.ToSignal(tree.CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
        }
    }
}
namespace MegaCrit.Sts2.Core.Entities.Creatures { public class Creature { } }
namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities { public enum ShakeStrength { Weak } public enum ShakeDuration { Short } }
namespace MegaCrit.Sts2.Core.Nodes
{
    public class NGame { public static NGame? Instance => null; public void ScreenShake(Vfx.Utilities.ShakeStrength s, Vfx.Utilities.ShakeDuration d) { } }
}
namespace MegaCrit.Sts2.Core.Nodes.Rooms
{
    public class NCombatRoom
    {
        public static NCombatRoom? Instance { get; set; }
        public Control CombatVfxContainer { get; } = new();
        public FakeCreature GetCreatureNode(Entities.Creatures.Creature creature) => new();
    }
    public class FakeCreature { public Vector2 VfxSpawnPosition => new(300, 340); }
}
namespace MegaCrit.Sts2.Core.Settings { public enum FastModeType { Normal, Fast, Instant } }
namespace MegaCrit.Sts2.Core.Saves
{
    public class SaveManager { public static SaveManager Instance { get; } = new(); public Prefs PrefsSave { get; } = new(); }
    public class Prefs { public Settings.FastModeType FastMode; public bool TextEffectsEnabled = true; public int ScreenShakeOptionIndex = 2; }
}
