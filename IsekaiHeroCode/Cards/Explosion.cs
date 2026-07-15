using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class Explosion() : IsekaiHeroCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(28m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "EXPLOSION!",
        "# Deal !Damage! damage to ALL enemies. You cannot play Attacks next turn.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = CombatState ?? throw new InvalidOperationException("EXPLOSION! requires an active combat.");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);

        await PowerCmd.Apply<ExplosionFatiguePower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
    }
}
