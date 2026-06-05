using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class ItemBox() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new CardsVar(1)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Item Box",
        "# Gain !Block! Block. Add Retain to {IfUpgraded:show:2 cards|a card} in your Hand.",
        ("selectionScreenPrompt", "Choose a card to Retain."));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        var owner = Owner;
        if (owner == null)
            return;

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, DynamicVars.Cards.IntValue),
            card => !card.Keywords.Contains(CardKeyword.Retain),
            this);

        foreach (var card in selectedCards)
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
