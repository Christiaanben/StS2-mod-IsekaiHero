using System;
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

public sealed class LastHitBonus() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new CardsVar(1),
        new EnergyVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal),
        EnergyHoverTip
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Last-Hit Bonus",
        "# Deal !Damage! damage. Fatal: Draw !Cards! {IfUpgraded:show:cards|card} and gain !Energy! Energy.");

    // Use the generic portrait until dedicated Last-Hit Bonus art is added.

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var shouldTriggerFatal = play.Target.Powers.All(power => power.ShouldOwnerDeathTriggerFatal());
        var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var killedTarget = shouldTriggerFatal &&
                           attack.Results.SelectMany(results => results).Any(result => result.WasTargetKilled);
        if (!IsConditionalEffectActive(killedTarget))
            return;

        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
