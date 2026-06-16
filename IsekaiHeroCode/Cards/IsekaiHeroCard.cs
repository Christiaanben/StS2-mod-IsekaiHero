using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Character;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Runtime.CompilerServices;

namespace IsekaiHero.IsekaiHeroCode.Cards;

[Pool(typeof(IsekaiHeroCardPool))]
public abstract class IsekaiHeroCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    private static readonly ConditionalWeakTable<IsekaiHeroCard, ConditionalOverride> ConditionalOverrides = new();
    private static readonly ConditionalWeakTable<IsekaiHeroCard, ConditionalTriggerState> ConditionalTriggers = new();

    public virtual bool HasConditionalEffects => false;

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
        var isActive = condition ||
               (CombatState != null &&
                ConditionalOverrides.TryGetValue(this, out var conditionalOverride) &&
                ReferenceEquals(conditionalOverride.CombatState, CombatState));

        if (isActive && CombatState != null)
            MarkConditionalEffectTriggered();

        return isActive;
    }

    public bool DidTriggerConditionalEffectThisPlay()
    {
        return ConditionalTriggers.TryGetValue(this, out var triggerState) &&
               ReferenceEquals(triggerState.CombatState, CombatState);
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card == this)
            ConditionalTriggers.Remove(this);

        return base.BeforeCardPlayed(cardPlay);
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
}
