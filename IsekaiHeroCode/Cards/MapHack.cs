using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class MapHack() : IsekaiHeroCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override List<(string, string)> Localization => new CardLoc(
        "Map Hack",
        "# Draw !Cards! cards, then discard 1 card.",
        ("selectionScreenPrompt", "Choose a card to discard."));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        var hand = CardPile.Get(PileType.Hand, Owner);
        if (hand == null || hand.Cards.Count == 0)
            return;

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            null,
            this);

        var selectedCard = selectedCards.FirstOrDefault();
        if (selectedCard != null)
            await CardPileCmd.Add(selectedCard, PileType.Discard);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
