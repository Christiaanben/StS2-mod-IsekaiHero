using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Character;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using IsekaiHero.IsekaiHeroCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Runtime.CompilerServices;

namespace IsekaiHero.IsekaiHeroCode.Cards;

[Pool(typeof(IsekaiHeroCardPool))]
public abstract class IsekaiHeroCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    private static readonly ConditionalWeakTable<IsekaiHeroCard, ConditionalOverride> ConditionalOverrides = new();
    private static readonly ConditionalWeakTable<IsekaiHeroCard, ConditionalTriggerState> ConditionalTriggers = new();
    private static readonly ConditionalWeakTable<IsekaiHeroCard, ExploitConsumptionState> ExploitConsumptions = new();
    private bool _resolvingPlay;
    private bool _usedLuckThisPlay;
    private bool _usedFormThisPlay;
    private bool _stackSettled;

    public virtual bool HasConditionalEffects => false;

    // The hand renderer polls the native hook and supplies the gold outline animation.
    protected override bool ShouldGlowGoldInternal => Owner != null && CombatState != null &&
        HasConditionalEffects && (CanBypassCondition || IsNaturalConditionMet);

    // Fatal conditions are outcomes of a play, so only guaranteed bypasses preview them.
    protected virtual bool IsNaturalConditionMet => false;

    private bool HasConditionOverride => CombatState != null &&
        ConditionalOverrides.TryGetValue(this, out var conditionalOverride) &&
        ReferenceEquals(conditionalOverride.CombatState, CombatState);

    private bool CanBypassCondition => HasConditionOverride ||
        Owner.Relics.OfType<BeginnersLuckCharm>().Any(relic => relic.CanUse) ||
        Owner.Creature.Powers.OfType<AscendantFormPower>().Any(power => power.Remaining > 0) ||
        Owner.Creature.Powers.OfType<ExploitPower>().Any(power => power.Amount > 0);

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HasConditionalEffects
        ? [HoverTipFactory.FromPower<ExploitPower>()]
        : [];

    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    
    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190
    
    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected bool IsConditionalEffectActive(bool condition)
    {
        var hasOverride = HasConditionOverride;
        // Hover/preview queries must never reserve a bypass or emit a trigger.
        if (!_resolvingPlay)
            return condition || CanBypassCondition;

        var usedLuck = !condition && !hasOverride && CombatState != null &&
                       (_usedLuckThisPlay || Owner.Relics.OfType<BeginnersLuckCharm>().Any(relic => relic.TryUse()));
        _usedLuckThisPlay |= usedLuck;
        var usedForm = !condition && !hasOverride && !usedLuck && CombatState != null &&
                       (_usedFormThisPlay || Owner.Creature.Powers.OfType<AscendantFormPower>().Any(power => power.TryUse()));
        _usedFormThisPlay |= usedForm;
        var exploitPower = !condition && !hasOverride && !usedLuck && !usedForm
            ? Owner?.Creature.Powers.OfType<ExploitPower>().FirstOrDefault(power => power.Amount > 0m)
            : null;
        var isActive = condition || hasOverride || usedLuck || usedForm || exploitPower != null;

        if (exploitPower != null && CombatState != null)
        {
            ExploitConsumptions.Remove(this);
            ExploitConsumptions.Add(this, new ExploitConsumptionState(CombatState));
        }

        if (isActive && CombatState != null)
            MarkConditionalEffectTriggered();

        return isActive;
    }

    public bool DidTriggerConditionalEffectThisPlay()
    {
        return ConditionalTriggers.TryGetValue(this, out var triggerState) &&
               ReferenceEquals(triggerState.CombatState, CombatState);
    }

    public bool DidConsumeExploitThisPlay()
    {
        return ExploitConsumptions.TryGetValue(this, out var consumptionState) &&
               ReferenceEquals(consumptionState.CombatState, CombatState);
    }

    internal bool SettleExploitThisPlay()
    {
        if (_stackSettled || !DidConsumeExploitThisPlay()) return false;
        _stackSettled = true;
        return true;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card == this)
        {
            _resolvingPlay = true;
            _usedLuckThisPlay = false;
            _usedFormThisPlay = false;
            _stackSettled = false;
            ConditionalTriggers.Remove(this);
            ExploitConsumptions.Remove(this);
        }

        return base.BeforeCardPlayed(cardPlay);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == this)
            _resolvingPlay = false;
        return base.AfterCardPlayed(choiceContext, cardPlay);
    }

    public void EnableConditionalEffectsForCombat()
    {
        if (CombatState == null)
            return;

        ConditionalOverrides.Remove(this);
        ConditionalOverrides.Add(this, new ConditionalOverride(CombatState));
        CardCmd.ApplyKeyword(this, IsekaiHeroKeywords.Override);
    }

    private void MarkConditionalEffectTriggered()
    {
        ConditionalTriggers.Remove(this);
        ConditionalTriggers.Add(this, new ConditionalTriggerState(CombatState!));
    }

    private sealed record ConditionalOverride(object CombatState);

    private sealed record ConditionalTriggerState(object CombatState);

    private sealed record ExploitConsumptionState(object CombatState);
}
