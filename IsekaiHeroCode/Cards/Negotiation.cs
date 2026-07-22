using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class Negotiation() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(2m),
        new DynamicVar("Exp", 1m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Negotiation",
        "# Apply !WeakPower! Weak. Gain !Exp! EXP.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            play.Target,
            DynamicVars.Weak.IntValue,
            Owner.Creature,
            this,
            false);

        await LevelPower.GainExp(choiceContext, Owner.Creature, DynamicVars["Exp"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
        DynamicVars["Exp"].UpgradeValueBy(1m);
    }
}
