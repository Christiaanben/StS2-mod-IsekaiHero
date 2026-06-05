using System.Linq;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class StatusAppraisal() : IsekaiHeroCard(0, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    public override List<(string, string)> Localization => new CardLoc(
        "Status Appraisal",
        "# Look at the top !Cards! cards of your draw pile. Put one in your hand and discard the others.",
        ("selectionScreenPrompt", "Choose a card to add to your hand."));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner == null)
            return;

        var drawPile = CardPile.Get(PileType.Draw, owner);
        if (drawPile == null)
            return;

        var topCards = drawPile.Cards.Take(DynamicVars.Cards.IntValue).ToList();
        if (topCards.Count == 0)
            return;

        var selectedCards = topCards.Count == 1
            ? [topCards[0]]
            : await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                topCards,
                owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1));

        var selectedCard = selectedCards.FirstOrDefault() ?? topCards[0];

        var discardedCards = topCards.Where(card => !ReferenceEquals(card, selectedCard)).ToArray();

        await CardPileCmd.Add(selectedCard, PileType.Hand);

        if (discardedCards.Length > 0)
            await CardPileCmd.Add(discardedCards, PileType.Discard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}
