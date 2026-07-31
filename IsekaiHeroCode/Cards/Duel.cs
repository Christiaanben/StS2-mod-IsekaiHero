using System;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class Duel() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new ExtraDamageVar(6m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Duel",
        "# Deal !Damage! damage. If only one enemy remains, deal !ExtraDamage! more damage.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var combatState = CombatState ?? throw new InvalidOperationException("Duel requires an active combat.");
        var damage = DynamicVars.Damage.BaseValue;

        if (IsConditionalEffectActive(combatState.HittableEnemies.Count() == 1))
            damage += DynamicVars.ExtraDamage.BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }
}
