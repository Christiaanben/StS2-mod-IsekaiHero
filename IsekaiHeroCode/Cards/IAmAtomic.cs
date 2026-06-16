using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class IAmAtomic() : IsekaiHeroCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(36m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "I Am Atomic",
        "# {IfUpgraded:show:Apply !VulnerablePower! Vulnerable and deal|Deal} !Damage! damage to ALL enemies. Costs 1 less this combat whenever you trigger a conditional effect.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = CombatState ?? throw new InvalidOperationException("I Am Atomic requires an active combat.");

        if (IsUpgraded)
        {
            await PowerCmd.Apply<VulnerablePower>(
                combatState.Enemies,
                DynamicVars.Vulnerable.IntValue,
                Owner.Creature,
                this,
                false);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner &&
            cardPlay.Card is IsekaiHeroCard { HasConditionalEffects: true } conditionalCard &&
            conditionalCard.DidTriggerConditionalEffectThisPlay())
        {
            ReduceCostForConditionalTrigger();
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }

    private void ReduceCostForConditionalTrigger()
    {
        if (CombatState == null || EnergyCost.CostsX)
            return;

        EnergyCost.AddThisCombat(-1);
    }
}
