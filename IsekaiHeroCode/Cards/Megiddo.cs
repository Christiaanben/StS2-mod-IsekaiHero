using System;
using System.Linq;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class Megiddo() : IsekaiHeroCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18m, ValueProp.Move),
        new DamageVar("PowerDamage", 9m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Megiddo",
        "# Deal !Damage! damage. If you played a Power this turn, deal !PowerDamage! damage to ALL enemies.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var combatState = CombatState ?? throw new InvalidOperationException("Megiddo requires an active combat.");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (!PlayedPowerThisTurn())
            return;

        await DamageCmd.Attack(DynamicVars["PowerDamage"].BaseValue).FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
        DynamicVars["PowerDamage"].UpgradeValueBy(3m);
    }

    private bool PlayedPowerThisTurn()
    {
        return CombatManager.Instance.History.CardPlaysFinished.Any(entry =>
            entry.HappenedThisTurn(CombatState) &&
            entry.CardPlay.Card.Owner == Owner &&
            entry.CardPlay.Card.Type == CardType.Power);
    }
}
