using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace IsekaiHero.IsekaiHeroCode.Relics;

public sealed class ForbiddenWalkthrough : IsekaiHeroRelic
{
    private bool _granted;

    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool ShouldReceiveCombatHooks => true;
    public override List<(string, string)> Localization => new RelicLoc(
        "Forbidden Walkthrough",
        "At the start of each combat, gain 3 Exploit.",
        "This world hasn't been released yet. Who wrote the guide?");
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ExploitPower>()];

    public override Task BeforeCombatStart()
    {
        _granted = false;
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (_granted || side != Owner.Creature.Side || !participants.Contains(Owner.Creature) || combatState.RoundNumber != 1)
            return;

        _granted = true;
        Flash();
        await PowerCmd.Apply<ExploitPower>(choiceContext, Owner.Creature, 3, Owner.Creature, null);
    }
}
