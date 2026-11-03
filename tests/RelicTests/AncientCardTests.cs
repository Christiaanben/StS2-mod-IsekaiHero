using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Cards;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

static class AncientCardTests
{
    static readonly PlayerChoiceContext Context = new();
    static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}; got {actual}");
    }
    static Player Player()
    {
        var p = new Player();
        p.Creature.CombatState = new CombatState();
        p.Creature.CombatState.Enemies.Add(new Creature { Side = CombatSide.Enemy });
        return p;
    }
    static T Card<T>(Player p) where T : CardModel, new()
    {
        var card = new T(); p.Creature.CombatState!.AddCard(card, p); return card;
    }
    static void Finished(CardModel card) => CombatManager.Instance.History.CardPlaysFinished.Add(
        new HistoryEntry(new CardPlay(card), card.CombatState!));
    static Task Play(CardModel card, Creature? target = null) => card.Play(Context, new CardPlay(card, target));

    public static IEnumerable<(string, Func<Task>)> All()
    {
        yield return ("Ancient registrations replace Grind and reserve the Tome for NEW GAME+", () =>
        {
            Equal(typeof(PowerLeveling), ((ITranscendenceCard)new Grind()).GetTranscendenceTransformedCard().GetType());
            Equal(true, new NewGamePlus() is ITomeCard);
            Equal(false, typeof(ITomeCard).IsAssignableFrom(typeof(AscendantForm)));
            Equal(true, new NewGamePlus().CanonicalKeywords.Contains(CardKeyword.Exhaust));
            Equal(false, new PowerLeveling().CanonicalKeywords.Contains(CardKeyword.Exhaust));
            return Task.CompletedTask;
        });
        foreach (var upgraded in new[] { false, true })
            yield return ($"Power Leveling{(upgraded ? "+" : "")} grants EXP and stats before both hits", async () =>
            {
                var p = Player(); var card = Card<PowerLeveling>(p);
                if (upgraded) card.Upgrade();
                CreatureCmd.OnDamage = hit =>
                {
                    Equal(upgraded ? 4 : 3, LevelPower.GetLevel(p.Creature));
                    Equal(upgraded ? 2 : 1, p.Creature.Powers.OfType<StrengthPower>().Sum(x => x.Amount));
                    Equal(upgraded ? 8m : 6m, hit.Damage);
                    return Task.CompletedTask;
                };
                await Play(card, p.Creature.CombatState!.Enemies[0]);
                Equal(2, CreatureCmd.Hits.Count);
            });
        yield return ("Power Leveling grants milestone energy before attacking and banks capped EXP", async () =>
        {
            var p = Player(); var card = Card<PowerLeveling>(p);
            await LevelPower.GainExp(Context, p.Creature, 8, null);
            CreatureCmd.OnDamage = _ => { Equal(1m, p.Energy); return Task.CompletedTask; };
            await Play(card, p.Creature.CombatState!.Enemies[0]);
            Equal(5, LevelPower.GetLevel(p.Creature));
            CreatureCmd.OnDamage = null;
            await LevelPower.GainExp(Context, p.Creature, 20, null);
            await Play(card, p.Creature.CombatState.Enemies[0]);
            Equal(10, LevelPower.GetLevel(p.Creature)); Equal(2m, p.Energy);
            Equal(8, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
        });
        yield return ("NEW GAME+ filters owner, turn, resolving cards and repeated plays by instance", async () =>
        {
            var p = Player(); var other = Player(); var ng = Card<NewGamePlus>(p);
            var old = Card<Grind>(p); Finished(old); p.Creature.CombatState!.RoundNumber++;
            var a = Card<Grind>(p); var b = Card<Grind>(p); Finished(a); Finished(a); Finished(b);
            Finished(Card<Grind>(other)); Finished(ng);
            var resolving = Card<Grind>(p); Finished(resolving); await CardPileCmd.Add(resolving, PileType.Play);
            CardSelectCmd.Select = cards => cards;
            await Play(ng);
            Equal(2, CardSelectCmd.LastCandidates.Count);
            Equal(true, p.Piles[PileType.Hand].Cards.Contains(a));
            Equal(true, p.Piles[PileType.Hand].Cards.Contains(b));
        });
        yield return ("NEW GAME+ restores exhausted cards and the actual removed Power without clearing its effect", async () =>
        {
            var p = Player(); var ng = Card<NewGamePlus>(p);
            var attack = Card<IAmAtomic>(p); Finished(attack); await CardPileCmd.Add(attack, PileType.Exhaust);
            var power = Card<AscendantForm>(p); await Play(power); Finished(power); power.HasBeenRemovedFromState = true;
            var effect = p.Creature.Powers.OfType<AscendantFormPower>().Single();
            CardSelectCmd.Select = cards => cards;
            await Play(ng);
            Equal(false, power.HasBeenRemovedFromState);
            Equal(true, p.Piles[PileType.Hand].Cards.Contains(power));
            Equal(true, p.Piles[PileType.Hand].Cards.Contains(attack));
            Equal(true, ReferenceEquals(effect, p.Creature.Powers.OfType<AscendantFormPower>().Single()));
            Equal(0, power.EnergyCost.TurnCost!.Value);
            power.EnergyCost.AfterCardPlayedCleanup(); Equal(0, power.EnergyCost.TurnCost!.Value);
            power.EnergyCost.EndOfTurnCleanup(); Equal(null, power.EnergyCost.TurnCost);
        });
        foreach (var upgraded in new[] { false, true })
            yield return ($"NEW GAME+{(upgraded ? "+" : "")} selection limit and optional skip", async () =>
            {
                var p = Player(); var ng = Card<NewGamePlus>(p); if (upgraded) ng.Upgrade();
                for (int i = 0; i < 5; i++) Finished(Card<Grind>(p));
                CardSelectCmd.Select = _ => []; await Play(ng); Equal(0, p.Piles[PileType.Hand].Cards.Count);
                Equal(upgraded ? 4 : 3, CardSelectCmd.LastMax);
                CardSelectCmd.Select = cards => cards; await Play(ng);
                Equal(upgraded ? 4 : 3, p.Piles[PileType.Hand].Cards.Count);
            });
        yield return ("NEW GAME+ allows earlier copies to retrieve one another and never the resolving copy", async () =>
        {
            var p = Player(); var a = Card<NewGamePlus>(p); var b = Card<NewGamePlus>(p);
            Finished(a); await CardPileCmd.Add(a, PileType.Exhaust);
            CardSelectCmd.Select = cards => cards; await Play(b);
            Equal(true, p.Piles[PileType.Hand].Cards.Contains(a));
            Finished(b); await CardPileCmd.Add(b, PileType.Exhaust); await Play(a);
            Equal(true, p.Piles[PileType.Hand].Cards.Contains(b));
            Equal(false, CardSelectCmd.LastCandidates.Contains(a));
        });
        yield return ("NEW GAME+ discounts held cards without duplication, preserves X costs and respects overflow", async () =>
        {
            var p = Player(); var ng = Card<NewGamePlus>(p);
            var held = Card<Grind>(p); Finished(held); await CardPileCmd.Add(held, PileType.Hand);
            var x = Card<Grind>(p); x.EnergyCost.CostsX = true; Finished(x);
            for (int i = 0; i < 9; i++) await CardPileCmd.Add(Card<Grind>(p), PileType.Hand);
            CardSelectCmd.Select = cards => cards; await Play(ng);
            Equal(1, p.Piles[PileType.Hand].Cards.Count(card => ReferenceEquals(card, held)));
            Equal(0, held.EnergyCost.TurnCost!.Value); Equal(null, x.EnergyCost.TurnCost);
            Equal(PileType.Discard, x.Pile!.Type);
            p.Creature.CombatState!.RoundNumber++;
            await Play(ng); // Old entries cannot be selected in a later turn.
            var next = Player(); var nextNg = Card<NewGamePlus>(next); await Play(nextNg);
            Equal(0, next.Piles[PileType.Hand].Cards.Count);
        });
        yield return ("NEW GAME+ ignores another side's plays and stops when combat ends during selection", async () =>
        {
            var p = Player(); var ng = Card<NewGamePlus>(p);
            p.Creature.CombatState!.CurrentSide = CombatSide.Enemy;
            var enemyTurnCard = Card<Grind>(p); Finished(enemyTurnCard);
            p.Creature.CombatState.CurrentSide = CombatSide.Player;
            var eligible = Card<Grind>(p); Finished(eligible);
            CardSelectCmd.Select = cards =>
            {
                Equal(1, cards.Count()); Equal(true, ReferenceEquals(eligible, cards.Single()));
                CombatManager.Instance.IsEnding = true;
                return cards;
            };
            await Play(ng); Equal(0, p.Piles[PileType.Hand].Cards.Count);
            Equal(null, eligible.EnergyCost.TurnCost);
        });
        yield return ("Power Leveling does not attack after a Level-Up reaction ends combat", async () =>
        {
            var p = Player(); var card = Card<PowerLeveling>(p);
            var relic = new IsekaiHero.IsekaiHeroCode.Relics.OpSmartphone { Owner = p }; p.Relics.Add(relic);
            CreatureCmd.OnDamage = _ => { CombatManager.Instance.IsInProgress = false; return Task.CompletedTask; };
            await Play(card, p.Creature.CombatState!.Enemies[0]);
            Equal(1, CreatureCmd.Hits.Count); // Only the relic's reaction, never the card's attack.
        });
    }
}
