using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace IsekaiHero.IsekaiHeroCode.Relics;

public sealed class BeginnersLuckCharm : IsekaiHeroRelic
{
    private bool _used;

    public override RelicRarity Rarity => RelicRarity.Common;
    public override bool ShouldReceiveCombatHooks => true;
    public override List<(string, string)> Localization => new RelicLoc(
        "Beginner's Luck Charm",
        "Your first unmet Exploit condition each combat counts as met.",
        "The warranty doesn't cover miracles.");
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ExploitPower>()];

    public override Task BeforeCombatStart()
    {
        _used = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    // Called only while resolving a card play, after natural conditions and Override.
    internal bool TryUse()
    {
        if (_used)
            return false;

        _used = true;
        Status = RelicStatus.Disabled;
        Flash();
        return true;
    }
}
