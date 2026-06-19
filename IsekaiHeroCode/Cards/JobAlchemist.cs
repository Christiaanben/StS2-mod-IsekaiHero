using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class JobAlchemist() : IsekaiHeroCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Amount", 4m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Job: Alchemist",
        "# The first time each turn you apply a debuff, gain !Amount! Block and your next Attack against that enemy deals !Amount! more damage.");


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<AlchemistPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["Amount"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Amount"].UpgradeValueBy(2m);
    }
}
