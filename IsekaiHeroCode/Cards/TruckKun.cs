using System.Linq;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class TruckKun() : IsekaiHeroCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    public override bool HasConditionalEffects => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new EnergyVar(1),
        new CardsVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        EnergyHoverTip
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Truck-kun",
        "# Deal !Damage! damage to ALL enemies. Fatal: Gain !Energy! Energy{IfUpgraded:show: and draw !Cards! card|}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = CombatState ?? throw new InvalidOperationException("Truck-kun requires an active combat.");
        var fatalEligibleEnemies = combatState.HittableEnemies
            .Where(enemy => enemy.Powers.All(power => power.ShouldOwnerDeathTriggerFatal()))
            .ToHashSet();

        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .TargetingAllOpponents(combatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);

        var killedEnemy = attack.Results.SelectMany(results => results).Any(result =>
            result.WasTargetKilled && fatalEligibleEnemies.Contains(result.Receiver));
        if (!IsConditionalEffectActive(killedEnemy))
            return;

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

        if (IsUpgraded)
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
