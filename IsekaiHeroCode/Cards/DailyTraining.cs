using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class DailyTraining() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new DynamicVar("Exp", 1m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Daily Training",
        "# Gain !Block! Block. Gain !Exp! EXP.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);
        await LevelPower.GainExp(choiceContext, Owner.Creature, DynamicVars["Exp"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Exp"].UpgradeValueBy(1m);
    }
}
