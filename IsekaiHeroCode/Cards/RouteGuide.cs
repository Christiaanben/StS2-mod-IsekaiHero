using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class RouteGuide() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new CardsVar(4),
        new CardsVar("TopCards", 1)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Route Guide",
        "# Gain !Block! Block. Look at the top !Cards! cards of your draw pile. Put !TopCards! on top and the rest at the bottom.",
        ("selectionScreenPrompt", "Choose the next card to draw."),
        ("secondSelectionScreenPrompt", "Choose the second card to draw, or skip."));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        var owner = Owner;
        if (owner == null)
            return;

        var drawPile = CardPile.Get(PileType.Draw, owner);
        if (drawPile == null)
            return;

        var topCards = drawPile.Cards.Take(DynamicVars.Cards.IntValue).ToList();
        if (topCards.Count <= 1)
            return;

        var firstCard = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            topCards,
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        if (firstCard == null)
            return;

        var cardsToKeepOnTop = new List<CardModel> { firstCard };

        if (DynamicVars["TopCards"].IntValue > 1)
        {
            var remainingCards = topCards.Where(card => !ReferenceEquals(card, firstCard)).ToList();
            var secondCard = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                remainingCards,
                owner,
                new CardSelectorPrefs(SecondSelectionScreenPrompt, 0, 1)
                {
                    Cancelable = true
                })).FirstOrDefault();

            if (secondCard != null)
                cardsToKeepOnTop.Add(secondCard);
        }

        var cardsToMoveToBottom = topCards
            .Where(card => !cardsToKeepOnTop.Any(selected => ReferenceEquals(card, selected)))
            .ToList();

        foreach (var card in cardsToMoveToBottom)
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);

        foreach (var card in cardsToKeepOnTop.AsEnumerable().Reverse())
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["TopCards"].UpgradeValueBy(1m);
    }

    private LocString SecondSelectionScreenPrompt =>
        new("cards", Id.Entry + ".secondSelectionScreenPrompt");
}
