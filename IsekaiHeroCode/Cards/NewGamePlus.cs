using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class NewGamePlus() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Ancient, TargetType.None), ITomeCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    public override List<(string, string)> Localization => new CardLoc(
        "NEW GAME+",
        "# Choose up to !Cards! cards you played this turn.\nReturn them to your hand. They cost 0 this turn.",
        ("selectionScreenPrompt", "Choose cards to play again."));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combat = CombatState;
        if (combat == null || CombatManager.Instance.IsOverOrEnding)
            return;
        var candidates = CombatManager.Instance.History.CardPlaysFinished
            .Where(entry => entry.HappenedThisTurn(combat) && entry.CardPlay.Card.Owner == Owner)
            .Select(entry => entry.CardPlay.Card)
            .Where(card => !ReferenceEquals(card, this) && card.Pile?.Type != PileType.Play)
            .DistinctBy(card => card, ReferenceEqualityComparer.Instance).ToList();
        if (candidates.Count == 0)
            return;

        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, Math.Min(DynamicVars.Cards.IntValue, candidates.Count))
            { Cancelable = true });
        foreach (var card in selected)
        {
            if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
                break;
            // Played Powers remain in the combat's card registry but are marked removed.
            // Restore that same instance, retaining the Power effect already on the player.
            card.HasBeenRemovedFromState = false;
            if (!combat.ContainsCard(card))
                combat.AddCard(card, Owner);
            if (!card.EnergyCost.CostsX)
                card.EnergyCost.SetThisTurn(0);
            if (card.Pile?.Type != PileType.Hand)
                await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
