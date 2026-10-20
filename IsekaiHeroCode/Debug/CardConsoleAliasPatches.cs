using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

namespace IsekaiHero.IsekaiHeroCode.Debug;

[HarmonyPatch]
internal static class CardConsoleAliasProcessPatch
{
    private static IEnumerable<MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(CardConsoleCmd), nameof(CardConsoleCmd.Process)),
        AccessTools.Method(typeof(RemoveCardConsoleCmd), nameof(RemoveCardConsoleCmd.Process))
    ];

    [HarmonyPrefix]
    private static void Prefix(ref string[] args) => args = CardConsoleAliases.Resolve(args);
}

[HarmonyPatch]
internal static class CardConsoleAliasCompletionPatch
{
    private static IEnumerable<MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(CardConsoleCmd), nameof(CardConsoleCmd.GetArgumentCompletions)),
        AccessTools.Method(typeof(RemoveCardConsoleCmd), nameof(RemoveCardConsoleCmd.GetArgumentCompletions))
    ];

    [HarmonyPostfix]
    private static void Postfix(AbstractConsoleCmd __instance, string[] args, CompletionResult __result)
    {
        if (args.Length > 1) return; // Leave pile-name completion to the game.
        var aliases = CardConsoleAliases.Complete(args.FirstOrDefault() ?? "").ToArray();
        if (aliases.Length == 0) return;
        __result.Candidates = __result.Candidates.Concat(aliases).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        __result.CommonPrefix = CardConsoleAliases.CommonCompletion(__instance.CmdName + " ", __result.Candidates);
    }
}
