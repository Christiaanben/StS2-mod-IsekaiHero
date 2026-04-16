using System.Linq;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class StatusAppraisal() : IsekaiHeroCard(0, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override List<(string, string)> Localization => new CardLoc(
        "Status Appraisal",
        "Look at the top 3 cards of your draw pile. Put one in your hand and discard the others.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var owner = Owner;
        if (owner == null)
            return;

        var drawPile = CardPile.Get(PileType.Draw, owner);
        if (drawPile == null)
            return;

        var topCards = drawPile.Cards.Take(3).ToArray();
        if (topCards.Length == 0)
            return;

        var selectedCard = topCards.Length == 1
            ? topCards[0]
            : await CardSelectCmd.FromChooseACardScreen(choiceContext, topCards, owner, false) ?? topCards[0];

        var discardedCards = topCards.Where(card => !ReferenceEquals(card, selectedCard)).ToArray();

        await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Top, this, true);

        if (discardedCards.Length > 0)
            await CardPileCmd.Add(discardedCards, PileType.Discard, CardPilePosition.Top, this, true);
    }

    protected override void OnUpgrade()
    {
    }
}
