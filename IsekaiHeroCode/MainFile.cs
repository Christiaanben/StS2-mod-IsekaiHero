using Godot;
using HarmonyLib;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Modding;

namespace IsekaiHero.IsekaiHeroCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "IsekaiHero"; //Used for resource filepath

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        SimpleLoc.EnableSimpleLoc(ModId);

        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}
