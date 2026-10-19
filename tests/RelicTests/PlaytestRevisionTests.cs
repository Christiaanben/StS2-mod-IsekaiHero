using IsekaiHero.IsekaiHeroCode.Cards;
using IsekaiHero.IsekaiHeroCode.Powers;
using IsekaiHero.IsekaiHeroCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

static class PlaytestRevisionTests
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
        for (var i = 0; i < 2; i++)
            p.Creature.CombatState.Enemies.Add(new Creature { Side = CombatSide.Enemy, CurrentHp = 49 });
        return p;
    }
    static async Task Play(CardModel card, Creature? target = null)
    {
        var play = new CardPlay(card, target);
        await card.BeforeCardPlayed(play);
        await card.Play(Context, play);
        // Powers precede cards in the native hook snapshot.
        foreach (var power in card.Owner.Creature.Powers.ToArray())
            await power.AfterCardPlayed(Context, play);
        await card.AfterCardPlayed(Context, play);
    }
    static (Func<IsekaiHeroCard> Make, Action<Player, bool> Verify)[] ConditionalCards =>
    [
        (() => new DangerSense(), (p,u) => Equal(u ? 10m : 8m, p.Creature.Block)),
        (() => new UnderdogSpirit(), (p,u) => Equal(u ? 15m : 12m, CreatureCmd.Hits.Last().Damage)),
        (() => new BeginnerMagic(), (p,u) => Equal(u ? 12m : 9m, CreatureCmd.Hits.Last().Damage)),
        (() => new ShieldBash(), (p,u) => Equal(u ? 8m : 6m, p.Creature.Block)),
        (() => new TwinBlades(), (p,u) => Equal(3, CreatureCmd.Hits.Count)),
        (() => new BossTelegraph(), (p,u) => { Equal(5m, p.Creature.Block); Equal(u ? 1 : 0, CardPileCmd.Drawn); }),
        (() => new TutorialSword(), (p,u) => Equal(u ? 15m : 11m, CreatureCmd.Hits.Last().Damage)),
        (() => new EmergencyDodge(), (p,u) => Equal(u ? 8m : 6m, p.Creature.Block)),
        (() => new ComboRush(), (p,u) => Equal(3, CreatureCmd.Hits.Count)),
        (() => new RaidOpener(), (p,u) => Equal(u ? 28m : 23m, CreatureCmd.Hits.Last().Damage)),
        (() => new Duel(), (p,u) => Equal(u ? 18m : 14m, CreatureCmd.Hits.Last().Damage)),
        (() => new CounterRead(), (p,u) => Equal(u ? 3 : 2, p.Creature.CombatState!.Enemies[0].Powers.OfType<WeakPower>().Single().Amount)),
        (() => new SeenItComing(), (p,u) => Equal(u ? 2 : 1, p.Creature.CombatState!.Enemies[0].Powers.OfType<WeakPower>().Single().Amount)),
        (() => new BarrierMagic(), (p,u) => Equal(u ? 21m : 18m, p.Creature.Block)),
        (() => new StealTechnique(), (p,u) => Equal(u ? 3 : 2, p.Creature.Powers.OfType<LevelPower>().Single().Experience)),
        (() => new AntiBossArt(), (p,u) => Equal(u ? 36m : 30m, CreatureCmd.Hits.Last().Damage)),
        (() => new Megiddo(), (p,u) => Equal(3, CreatureCmd.Hits.Count)),
        (() => new TrainingArc(), (p,u) => Equal(4m, p.Creature.Block))
    ];

    public static IEnumerable<(string, Func<Task>)> All()
    {
        foreach (var (make, verify) in ConditionalCards)
        foreach (var upgraded in new[] { false, true })
        foreach (var source in new[] { "none", "stack", "Override", "luck", "Form" })
        {
            var name = make().GetType().Name;
            yield return ($"{name}{(upgraded ? "+" : "")} unmet condition via {source}", async () =>
            {
                var p = Player(); var card = make(); card.Owner = p;
                if (upgraded) card.Upgrade();
                if (source != "none")
                    await PowerCmd.Apply<ExploitPower>(Context, p.Creature, 2, p.Creature, null);
                if (source == "Override") card.EnableConditionalEffectsForCombat();
                if (source == "luck") p.Relics.Add(new BeginnersLuckCharm { Owner = p });
                if (source == "Form") await Play(new AscendantForm { Owner = p });
                await Play(card, p.Creature.CombatState!.Enemies[0]);
                Equal(source != "none", card.DidTriggerConditionalEffectThisPlay());
                Equal(source == "stack", card.DidConsumeExploitThisPlay());
                if (source != "none")
                {
                    Equal(source == "stack" ? 1 : 2, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
                    verify(p, upgraded);
                }
            });
        }

        foreach (var (make, _) in ConditionalCards)
        foreach (var upgraded in new[] { false, true })
            yield return ($"{make().GetType().Name}{(upgraded ? "+" : "")} natural success preserves bypasses", async () =>
            {
                var p = Player(); var combat = p.Creature.CombatState!;
                combat.Enemies.RemoveAt(1);
                var enemy = combat.Enemies[0]; enemy.CurrentHp = enemy.MaxHp; enemy.Monster!.IntendsToAttack = true;
                enemy.Monster.CombatState.Encounter = new Encounter { RoomType = RoomType.Elite };
                await PowerCmd.Apply<WeakPower>(Context, enemy, 1, p.Creature, null);
                await LevelPower.GainExp(Context, p.Creature, 12, null);
                p.Creature.Block = 1;
                CombatManager.Instance.History.CardPlaysFinished.Add(new(new CardPlay(new Negotiation { Owner = p }), combat));
                CombatManager.Instance.History.CardPlaysFinished.Add(new(new CardPlay(new JobAlchemist { Owner = p }), combat));
                await PowerCmd.Apply<ExploitPower>(Context, p.Creature, 2, p.Creature, null);
                var charm = new BeginnersLuckCharm { Owner = p }; p.Relics.Add(charm);
                await Play(new AscendantForm { Owner = p });
                var card = make(); card.Owner = p; if (upgraded) card.Upgrade();
                await Play(card, enemy);
                Equal(true, card.DidTriggerConditionalEffectThisPlay());
                Equal(false, card.DidConsumeExploitThisPlay()); Equal(0, charm.Flashes);
                Equal(2, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
                Equal(2, p.Creature.Powers.OfType<AscendantFormPower>().Single().Remaining);
            });

        yield return ("Alchemist grants visible Vigor once, stacks rewards without resetting, and refreshes owner only", async () =>
        {
            var p = Player(); var enemy = p.Creature.CombatState!.Enemies[0];
            var job = new JobAlchemist { Owner = p }; await Play(job);
            await Play(new Negotiation { Owner = p }, enemy);
            var alchemist = p.Creature.Powers.OfType<AlchemistPower>().Single();
            Equal(3m, p.Creature.Block); Equal(3, p.Creature.Powers.OfType<VigorPower>().Single().Amount);
            job.Upgrade(); await Play(job); await Play(new Negotiation { Owner = p }, enemy);
            Equal(8, alchemist.Amount); Equal(3m, p.Creature.Block);
            await alchemist.AfterPlayerTurnStart(Context, Player());
            await Play(new Negotiation { Owner = p }, enemy); Equal(3m, p.Creature.Block);
            await alchemist.AfterPlayerTurnStart(Context, p);
            await Play(new Negotiation { Owner = p }, enemy);
            Equal(11m, p.Creature.Block); Equal(11, p.Creature.Powers.OfType<VigorPower>().Single().Amount);
        });
        yield return ("Alchemist ignores resisted, removed, self and ally debuffs", async () =>
        {
            var p = Player(); await Play(new JobAlchemist { Owner = p });
            var alchemist = p.Creature.Powers.OfType<AlchemistPower>().Single();
            var enemy = p.Creature.CombatState!.Enemies[0];
            var weak = new WeakPower { Owner = enemy, Amount = 0 };
            await alchemist.AfterPowerAmountChanged(Context, weak, 0, p.Creature, null);
            weak.Amount = 1; await alchemist.AfterPowerAmountChanged(Context, weak, -1, p.Creature, null);
            await PowerCmd.Apply<WeakPower>(Context, p.Creature, 1, p.Creature, null);
            await alchemist.AfterPowerAmountChanged(Context, weak, 1, Player().Creature, null);
            Equal(0m, p.Creature.Block);
            await PowerCmd.Apply<WeakPower>(Context, enemy, 1, p.Creature, null);
            Equal(3m, p.Creature.Block);
        });
        yield return ("Tutorial Strike recognizes the real Alchemist Job and retains Strike tag", async () =>
        {
            var p = Player(); var card = new TutorialSword { Owner = p };
            await Play(new JobAlchemist { Owner = p });
            await Play(card, p.Creature.CombatState!.Enemies[0]);
            Equal(11m, CreatureCmd.Hits.Last().Damage); Equal(true, card.Tags.Contains(CardTag.Strike));
        });
        yield return ("Form precedence, duplicate use limits, upgraded limit, turn refresh and owner isolation", async () =>
        {
            var p = Player(); var card = new DangerSense { Owner = p }; var formCard = new AscendantForm { Owner = p };
            Equal(3, formCard.BaseCost); await Play(formCard);
            var form = p.Creature.Powers.OfType<AscendantFormPower>().Single();
            var charm = new BeginnersLuckCharm { Owner = p }; p.Relics.Add(charm);
            await PowerCmd.Apply<ExploitPower>(Context, p.Creature, 3, p.Creature, null);
            await Play(card); Equal(2, form.Remaining); Equal(1, charm.Flashes);
            await Play(card); Equal(1, form.Remaining); Equal(3, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
            await Play(formCard); Equal(1, form.Remaining);
            formCard.Upgrade(); await Play(formCard); Equal(2, form.Remaining);
            await Play(card); await Play(card); Equal(0, form.Remaining);
            await Play(card); Equal(2, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
            await form.AfterPlayerTurnStart(Context, Player()); Equal(0, form.Remaining);
            await form.AfterPlayerTurnStart(Context, p); Equal(3, form.Remaining);
            var ally = Player(); var allyCard = new DangerSense { Owner = ally };
            await Play(allyCard); Equal(false, allyCard.DidTriggerConditionalEffectThisPlay()); Equal(3, form.Remaining);
        });
        yield return ("Negotiation is free; upgrading increases Weak but not EXP", async () =>
        {
            var p = Player(); var card = new Negotiation { Owner = p }; card.Upgrade();
            Equal(0, card.BaseCost); await Play(card, p.Creature.CombatState!.Enemies[0]);
            Equal(1, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
            Equal(2, p.Creature.CombatState.Enemies[0].Powers.OfType<WeakPower>().Single().Amount);
        });
        yield return ("Breakthrough does not carry a Level Up across sides or credit capped EXP", async () =>
        {
            var p = Player(); await LevelPower.GainExp(Context, p.Creature, 36, null);
            Equal(true, LevelPower.LeveledUpThisTurn(p.Creature));
            p.Creature.CombatState!.CurrentSide = CombatSide.Enemy;
            Equal(false, LevelPower.LeveledUpThisTurn(p.Creature));
            p.Creature.CombatState.RoundNumber++;
            await LevelPower.GainExp(Context, p.Creature, 10, null);
            Equal(false, LevelPower.LeveledUpThisTurn(p.Creature));
        });
        yield return ("Atomic charges and detonates once per play, with compact replays", async () =>
        {
            var p = Player(); var card = new IAmAtomic { Owner = p }; card.Upgrade();
            var events = IsekaiHero.IsekaiHeroCode.Vfx.AtomicVfx.Events; events.Clear();
            CreatureCmd.OnDamage = _ =>
            {
                Equal("Charge,Detonate", string.Join(',', events));
                Equal(true, p.Creature.CombatState!.Enemies.All(e => e.Powers.OfType<VulnerablePower>().Any()));
                return Task.CompletedTask;
            };
            await Play(card); Equal(2, CreatureCmd.Hits.Count);
            CreatureCmd.OnDamage = null; events.Clear();
            var replay = new CardPlay(card) { PlayIndex = 1 };
            await card.BeforeCardPlayed(replay); await card.Play(Context, replay); await card.AfterCardPlayed(Context, replay);
            Equal("CompactCharge,Detonate", string.Join(',', events));
        });
    }
}
