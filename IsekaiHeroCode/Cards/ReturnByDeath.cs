using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class ReturnByDeath() : IsekaiHeroCard(2, CardType.Power, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(15m),
        new DynamicVar("Exp", 8m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Return by Death",
        "# Once per combat, if you would die, heal !Heal! HP and gain !Exp! EXP instead.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var power = await PowerCmd.Apply<ReturnByDeathPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this, false);
        power?.Configure(DynamicVars.Heal.IntValue, DynamicVars["Exp"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(5m);
        DynamicVars["Exp"].UpgradeValueBy(2m);
    }
}
