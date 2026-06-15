using System.Linq;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class SystemMenu() : IsekaiHeroCard(2, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override List<(string, string)> Localization => new CardLoc(
        "System Menu",
        "Choose a card in your hand{IfUpgraded:show: or discard pile|}. Add Override to it for the rest of combat.",
        ("selectionScreenPrompt", "Choose a card to Override."));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner == null)
            return;

        var candidates = CardPile.Get(PileType.Hand, owner)?.Cards
            .OfType<IsekaiHeroCard>()
            .Where(card => card.HasConditionalEffects)
            .ToList() ?? [];

        if (IsUpgraded)
        {
            var discardCandidates = CardPile.Get(PileType.Discard, owner)?.Cards
                .OfType<IsekaiHeroCard>()
                .Where(card => card.HasConditionalEffects) ?? [];

            candidates.AddRange(discardCandidates);
        }

        if (candidates.Count == 0)
            return;

        var selectedCards = candidates.Count == 1
            ? [candidates[0]]
            : await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                candidates,
                owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1));

        if (selectedCards.FirstOrDefault() is IsekaiHeroCard selectedCard)
            selectedCard.EnableConditionalEffectsForCombat();
    }

    protected override void OnUpgrade()
    {
    }
}
