using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class StudyTheSystem() : IsekaiHeroCard(0, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Exp", 2m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Study the System",
        "# Gain !Exp! EXP.");

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return LevelPower.GainExp(choiceContext, Owner.Creature, DynamicVars["Exp"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Exp"].UpgradeValueBy(1m);
    }
}
