namespace IsekaiHero.IsekaiHeroCode.Debug;

// Display-name aliases only. Canonical IDs stay unchanged for saved cards and art.
internal static class CardConsoleAliases
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TUTORIAL_STRIKE"] = "ISEKAIHERO-TUTORIAL_SWORD",
        ["ISEKAIHERO-TUTORIAL_STRIKE"] = "ISEKAIHERO-TUTORIAL_SWORD",
        ["BREAKTHROUGH_STRIKE"] = "ISEKAIHERO-COMBO_RUSH",
        ["ISEKAIHERO-BREAKTHROUGH_STRIKE"] = "ISEKAIHERO-COMBO_RUSH"
    };

    internal static string[] Resolve(string[] args)
    {
        if (args.Length == 0 || !Aliases.TryGetValue(args[0], out var id)) return args;
        var resolved = (string[])args.Clone();
        resolved[0] = id;
        return resolved;
    }

    internal static IEnumerable<string> Complete(string prefix) =>
        Aliases.Keys.Where(alias => alias.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    internal static string CommonCompletion(string commandPrefix, List<string> candidates)
    {
        if (candidates.Count == 0) return "";
        if (candidates.Count == 1) return commandPrefix + candidates[0] + " ";
        var first = candidates[0];
        var length = 0;
        while (length < first.Length && candidates.All(candidate => candidate.Length > length &&
               char.ToUpperInvariant(candidate[length]) == char.ToUpperInvariant(first[length]))) length++;
        return length == 0 ? "" : commandPrefix + first[..length];
    }
}
