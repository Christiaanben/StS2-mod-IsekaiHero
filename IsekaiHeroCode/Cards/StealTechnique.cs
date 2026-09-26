using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class StealTechnique() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Exp", 2m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Steal Technique",
        "# Deal !Damage! damage. Exploit (target has a debuff): gain !Exp! EXP.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var targetHasDebuff = play.Target.Powers.Any(power =>
            power.GetTypeForAmount(power.Amount) == PowerType.Debuff);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (IsConditionalEffectActive(targetHasDebuff))
            await LevelPower.GainExp(choiceContext, Owner.Creature, DynamicVars["Exp"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Exp"].UpgradeValueBy(1m);
    }
}
