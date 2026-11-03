using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class PowerLeveling() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Exp", 8m), new DamageVar(6m, ValueProp.Move)];

    public override List<(string, string)> Localization => new CardLoc(
        "Power Leveling", "# Gain !Exp! EXP.\nDeal !Damage! damage twice.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await LevelPower.GainExp(choiceContext, Owner.Creature, DynamicVars["Exp"].IntValue, this);
        if (CombatState == null || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || play.Target.IsDead)
            return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(2)
            .FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Exp"].UpgradeValueBy(4m);
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
