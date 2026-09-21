using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class LevelGrinding() : IsekaiHeroCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Exp", 6m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Level Grinding",
        "# Gain !Exp! EXP.");

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return LevelPower.GainExp(choiceContext, Owner.Creature, DynamicVars["Exp"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Exp"].UpgradeValueBy(2m);
    }
}
