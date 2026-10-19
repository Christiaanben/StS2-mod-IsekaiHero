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

static class CardRevisionTests
{
    static readonly PlayerChoiceContext Context = new();
    static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}; got {actual}");
    }
    static Player Player()
    {
        var player = new Player();
        player.Creature.CombatState = new CombatState();
        player.Creature.CombatState.Enemies.Add(new Creature());
        return player;
    }
    static async Task Play(CardModel card, Creature? target = null)
    {
        var play = new CardPlay(card, target);
        await card.BeforeCardPlayed(play);
        await card.Play(Context, play);
        await card.AfterCardPlayed(Context, play);
    }
    static async Task<ReturnByDeathPower> Rescue(Player player, bool upgraded = false)
    {
        var card = new ReturnByDeath { Owner = player };
        if (upgraded) card.Upgrade();
        await Play(card);
        return player.Creature.Powers.OfType<ReturnByDeathPower>().Single();
    }
    public static IEnumerable<(string, Func<Task>)> All()
    {
        yield return ("revised card numbers and upgrades", async () =>
        {
            var p = Player(); var enemy = p.Creature.CombatState!.Enemies[0];
            var underdog = new UnderdogSpirit { Owner = p };
            Equal(6m, underdog.DynamicVars.Damage.BaseValue); Equal(6m, underdog.DynamicVars.ExtraDamage.BaseValue);
            underdog.Upgrade(); Equal(8m, underdog.DynamicVars.Damage.BaseValue); Equal(7m, underdog.DynamicVars.ExtraDamage.BaseValue);
            var daily = new DailyTraining { Owner = p }; await Play(daily);
            Equal(6m, p.Creature.Block); Equal(1, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
            daily.Upgrade(); Equal(8m, daily.DynamicVars.Block.BaseValue); Equal(2, daily.DynamicVars["Exp"].IntValue);
            var farm = new FarmTheField { Owner = p }; Equal(1, farm.BaseCost);
            farm.Upgrade(); Equal(6m, farm.DynamicVars.Damage.BaseValue); Equal(3, farm.DynamicVars["Exp"].IntValue);
            var steal = new Steal { Owner = p }; await Play(steal, enemy); Equal(6, p.Gold);
            Equal(true, steal.CanonicalKeywords.Contains(CardKeyword.Exhaust)); steal.Upgrade();
            Equal(9m, steal.DynamicVars.Damage.BaseValue); Equal(9, steal.DynamicVars.Gold.IntValue);
            var tactics = new DegenerateTactics { Owner = p }; Equal(12, tactics.DynamicVars.Gold.IntValue);
            tactics.Upgrade(); Equal(16, tactics.DynamicVars.Gold.IntValue); Equal(10m, tactics.DynamicVars.Damage.BaseValue);
            Equal(true, tactics.CanonicalKeywords.Contains(CardKeyword.Exhaust));
        });
        yield return ("Map Hack upgrade retains without increasing draw", async () =>
        {
            var p = Player(); var card = new MapHack { Owner = p }; var discarded = new ShieldBash { Owner = p };
            p.Piles[PileType.Hand].Cards.Add(discarded); card.Upgrade();
            Equal(true, card.Keywords.Contains(CardKeyword.Retain)); Equal(2, card.DynamicVars.Cards.IntValue);
            await Play(card); Equal(2, CardPileCmd.Drawn); Equal(discarded, p.Piles[PileType.Discard].Cards.Single());
        });
        yield return ("System Menu costs one and upgrade permits discard selection", async () =>
        {
            var p = Player(); var card = new SystemMenu { Owner = p }; var target = new ShieldBash { Owner = p };
            Equal(1, card.BaseCost); p.Piles[PileType.Discard].Cards.Add(target);
            await Play(card); await Play(target, p.Creature.CombatState!.Enemies[0]);
            Equal(false, target.DidTriggerConditionalEffectThisPlay()); p.Creature.Block = 0;
            card.Upgrade(); await Play(card); await Play(target, p.Creature.CombatState.Enemies[0]);
            Equal(true, target.DidTriggerConditionalEffectThisPlay());
            Equal(true, card.CanonicalKeywords.Contains(CardKeyword.Exhaust));
        });
        yield return ("Level Grinding and Power-Up Montage keep their existing roles", () =>
        {
            var p = Player(); var grind = new LevelGrinding { Owner = p }; var montage = new PowerUpMontage { Owner = p };
            Equal(2, grind.BaseCost); Equal(2, montage.BaseCost);
            Equal(false, grind.CanonicalKeywords.Contains(CardKeyword.Exhaust));
            Equal(6, grind.DynamicVars["Exp"].IntValue); grind.Upgrade(); Equal(8, grind.DynamicVars["Exp"].IntValue);
            Equal(4, montage.DynamicVars["Exp"].IntValue); Equal(4m, montage.DynamicVars.Block.BaseValue);
            montage.Upgrade(); Equal(5, montage.DynamicVars["Exp"].IntValue); Equal(6m, montage.DynamicVars.Block.BaseValue);
            return Task.CompletedTask;
        });
        yield return ("Shield Bash does not enable its own condition", async () =>
        {
            var p = Player(); var card = new ShieldBash { Owner = p }; var enemy = p.Creature.CombatState!.Enemies[0];
            await Play(card, enemy); Equal(3m, p.Creature.Block); Equal(false, card.DidTriggerConditionalEffectThisPlay());
            p.Creature.Block = 1; await Play(card, enemy); Equal(7m, p.Creature.Block);
            card.Upgrade(); p.Creature.Block = 1; await Play(card, enemy); Equal(9m, p.Creature.Block);
        });
        yield return ("Shield Bash Exploit forces only the bonus", async () =>
        {
            var p = Player(); var card = new ShieldBash { Owner = p };
            await PowerCmd.Apply<ExploitPower>(Context, p.Creature, 2, p.Creature, null);
            await Play(card, p.Creature.CombatState!.Enemies[0]); Equal(6m, p.Creature.Block);
            var exploit = p.Creature.Powers.OfType<ExploitPower>().Single();
            await exploit.AfterCardPlayed(Context, new CardPlay(card)); Equal(1, exploit.Amount);
        });
        yield return ("intent bonuses inspect the selected enemy only", async () =>
        {
            foreach (var make in new Func<Player, CardModel>[] { p => new BossTelegraph { Owner = p }, p => new CounterRead { Owner = p }, p => new SeenItComing { Owner = p } })
            {
                var p = Player(); var idle = p.Creature.CombatState!.Enemies[0];
                var attacker = new Creature(); attacker.Monster!.IntendsToAttack = true;
                p.Creature.CombatState.Enemies.Add(attacker);
                var card = (IsekaiHeroCard)make(p); await Play(card, idle); Equal(false, card.DidTriggerConditionalEffectThisPlay());
                await Play(card, attacker); Equal(true, card.DidTriggerConditionalEffectThisPlay());
            }
        });
        yield return ("Tutorial Sword accepts Job or owner Power this turn", async () =>
        {
            var p = Player(); var card = new TutorialSword { Owner = p }; var enemy = p.Creature.CombatState!.Enemies[0];
            await Play(card, enemy); Equal(7m, CreatureCmd.Hits.Last().Damage);
            p.Creature.Powers.Add(new JobTestPower()); await Play(card, enemy); Equal(11m, CreatureCmd.Hits.Last().Damage);
            p.Creature.Powers.Clear();
            var ally = Player(); CombatManager.Instance.History.CardPlaysFinished.Add(new(new CardPlay(new ReturnByDeath { Owner = ally }), p.Creature.CombatState));
            await Play(card, enemy); Equal(7m, CreatureCmd.Hits.Last().Damage);
            CombatManager.Instance.History.CardPlaysFinished.Add(new(new CardPlay(new ReturnByDeath { Owner = p }), p.Creature.CombatState));
            await Play(card, enemy); Equal(11m, CreatureCmd.Hits.Last().Damage);
            p.Creature.CombatState.RoundNumber++; await Play(card, enemy); Equal(7m, CreatureCmd.Hits.Last().Damage);
        });
        yield return ("Combo Rush retains its Level 4 threshold", async () =>
        {
            var p = Player(); var card = new ComboRush { Owner = p }; var enemy = p.Creature.CombatState!.Enemies[0];
            await Play(card, enemy); Equal(2, CreatureCmd.Hits.Count);
            await LevelPower.GainExp(Context, p.Creature, 12, null);
            CreatureCmd.Hits.Clear(); card.Upgrade(); await Play(card, enemy);
            Equal(3, CreatureCmd.Hits.Count); Equal(true, CreatureCmd.Hits.All(h => h.Damage == 5));
        });
        yield return ("Judgment grows once per actual level only while held", async () =>
        {
            var p = Player(); var hand = p.Piles[PileType.Hand]; var card = new HerosJudgment { Owner = p };
            hand.Cards.Add(card); await LevelPower.GainExp(Context, p.Creature, 3, null); Equal(16m, card.DynamicVars.Damage.BaseValue);
            await LevelPower.GainExp(Context, p.Creature, 5, null); Equal(24m, card.DynamicVars.Damage.BaseValue);
            hand.Cards.Clear(); p.Piles[PileType.Discard].Cards.Add(card);
            await LevelPower.GainExp(Context, p.Creature, 4, null); Equal(24m, card.DynamicVars.Damage.BaseValue);
            p.Piles[PileType.Discard].Cards.Clear(); hand.Cards.Add(card); card.Upgrade();
            await LevelPower.GainExp(Context, p.Creature, 4, null); Equal(30m, card.DynamicVars.Damage.BaseValue);
            await Play(card, p.Creature.CombatState!.Enemies[0]); Equal(30m, CreatureCmd.Hits.Last().Damage);
            Equal(false, card.HasConditionalEffects); Equal(true, card.CanonicalKeywords.Contains(CardKeyword.Retain));
            await card.AfterCombatEnd(new CombatRoom()); Equal(16m, card.DynamicVars.Damage.BaseValue);
        });
        yield return ("Judgment ignores allies and capped EXP", async () =>
        {
            var p = Player(); var card = new HerosJudgment { Owner = p }; p.Piles[PileType.Hand].Cards.Add(card);
            await LevelPower.GainExp(Context, Player().Creature, 8, null); Equal(16m, card.DynamicVars.Damage.BaseValue);
            await LevelPower.GainExp(Context, p.Creature, 36, null); Equal(52m, card.DynamicVars.Damage.BaseValue);
            await LevelPower.GainExp(Context, p.Creature, 40, null); Equal(52m, card.DynamicVars.Damage.BaseValue);
        });
        yield return ("Judgment growth precedes relic reactions and snapshots each level", async () =>
        {
            var p = Player(); var card = new HerosJudgment { Owner = p }; var later = new HerosJudgment { Owner = p };
            p.Piles[PileType.Hand].Cards.Add(card); p.Relics.Add(new OpSmartphone { Owner = p });
            var calls = 0;
            CreatureCmd.OnDamage = _ =>
            {
                if (calls++ == 0) { Equal(20m, card.DynamicVars.Damage.BaseValue); p.Piles[PileType.Hand].Cards.Clear(); p.Piles[PileType.Hand].Cards.Add(later); }
                return Task.CompletedTask;
            };
            await LevelPower.GainExp(Context, p.Creature, 8, null);
            Equal(20m, card.DynamicVars.Damage.BaseValue); Equal(20m, later.DynamicVars.Damage.BaseValue);
        });
        yield return ("EXPLOSION damages before ending only its owner's turn", async () =>
        {
            var p = Player(); var card = new Explosion { Owner = p }; Equal(3, card.BaseCost);
            card.Upgrade(); await Play(card);
            Equal(44m, CreatureCmd.Hits.Last().Damage); Equal(p, PlayerCmd.EndedTurns.Single());
            Equal("Damage,EndTurn", string.Join(',', PowerCmd.Events)); Equal(0, p.Creature.Powers.Count);
        });
        yield return ("lethal EXPLOSION does not request another turn transition", async () =>
        {
            var p = Player(); var card = new Explosion { Owner = p };
            CreatureCmd.OnDamage = _ => { CombatManager.Instance.IsEnding = true; return Task.CompletedTask; };
            await Play(card); Equal(36m, CreatureCmd.Hits.Last().Damage); Equal(0, PlayerCmd.EndedTurns.Count);
        });
        yield return ("Return by Death is a 2-cost Power without Exhaust", async () =>
        {
            var p = Player(); var card = new ReturnByDeath { Owner = p };
            Equal(2, card.BaseCost); Equal(CardType.Power, card.Type); Equal(false, card.CanonicalKeywords.Contains(CardKeyword.Exhaust));
            var power = await Rescue(p); p.Creature.CurrentHp = 0;
            Equal(false, power.ShouldDie(p.Creature)); await power.AfterPreventingDeath(p.Creature);
            Equal(15, p.Creature.CurrentHp); Equal(3, LevelPower.GetLevel(p.Creature)); Equal(true, power.ShouldDie(p.Creature));
        });
        yield return ("rescue reserves its use before heal reactions", async () =>
        {
            var p = Player(); var power = await Rescue(p, true); p.Creature.CurrentHp = 0;
            CreatureCmd.OnHeal = () => { Equal(true, power.ShouldDie(p.Creature)); return Task.CompletedTask; };
            await power.AfterPreventingDeath(p.Creature); Equal(20, p.Creature.CurrentHp);
            Equal(2, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
            p.Creature.CurrentHp = 0; await power.AfterPreventingDeath(p.Creature); Equal(0, p.Creature.CurrentHp);
        });
        yield return ("duplicate and reapplied rescue copies cannot rearm a save", async () =>
        {
            var p = Player(); var power = await Rescue(p); await Rescue(p, true); await Rescue(p);
            p.Creature.CurrentHp = 0; await power.AfterPreventingDeath(p.Creature); Equal(20, p.Creature.CurrentHp);
            p.Creature.Powers.Remove(power); var reapplied = await Rescue(p, true);
            Equal(true, reapplied.ShouldDie(p.Creature));
            p.Creature.CombatState = new CombatState(); p.Creature.Powers.Clear();
            var nextCombat = await Rescue(p); Equal(false, nextCombat.ShouldDie(p.Creature));
            p.Creature.CurrentHp = 0; await nextCombat.AfterPreventingDeath(p.Creature); Equal(15, p.Creature.CurrentHp);
        });
        yield return ("rescue ignores another player's lethal event and caps healing", async () =>
        {
            var p = Player(); var ally = Player(); var power = await Rescue(p, true);
            Equal(true, power.ShouldDie(ally.Creature)); await power.AfterPreventingDeath(ally.Creature);
            Equal(false, power.ShouldDie(p.Creature)); p.Creature.MaxHp = 10; p.Creature.CurrentHp = 0;
            await power.AfterPreventingDeath(p.Creature); Equal(10, p.Creature.CurrentHp);
        });
        yield return ("Atomic upgrade applies Vulnerable before damage", async () =>
        {
            var p = Player(); var card = new IAmAtomic { Owner = p }; card.Upgrade(); await Play(card);
            Equal("VulnerablePower,Damage", string.Join(',', PowerCmd.Events));
            Equal(1, p.Creature.CombatState!.Enemies[0].Powers.OfType<VulnerablePower>().Single().Amount);
            Equal(36m, CreatureCmd.Hits.Single().Damage); Equal(true, card.CanonicalKeywords.Contains(CardKeyword.Exhaust));
        });
        yield return ("Atomic discounts expire on play without removing other modifiers", async () =>
        {
            var p = Player(); var atomic = new IAmAtomic { Owner = p }; var other = new IAmAtomic { Owner = p };
            var trigger = new ShieldBash { Owner = p }; p.Creature.Block = 1; await Play(trigger, p.Creature.CombatState!.Enemies[0]);
            atomic.EnergyCost.AddThisCombat(-1);
            await atomic.AfterCardPlayed(Context, new CardPlay(trigger)); Equal(-2, atomic.EnergyCost.Offset); Equal(0, other.EnergyCost.Offset);
            await other.AfterCardPlayed(Context, new CardPlay(trigger)); Equal(-1, other.EnergyCost.Offset);
            p.Creature.CombatState.RoundNumber++; p.Piles[PileType.Discard].Cards.Add(atomic);
            Equal(-2, atomic.EnergyCost.Offset); await Play(atomic); atomic.EnergyCost.AfterCardPlayedCleanup();
            Equal(-1, atomic.EnergyCost.Offset); Equal(-1, other.EnergyCost.Offset);
            await atomic.AfterCardPlayed(Context, new CardPlay(trigger)); Equal(-2, atomic.EnergyCost.Offset);
            var ally = Player(); var allyTrigger = new ShieldBash { Owner = ally }; ally.Creature.Block = 1;
            await Play(allyTrigger, ally.Creature.CombatState!.Enemies[0]); await atomic.AfterCardPlayed(Context, new CardPlay(allyTrigger));
            Equal(-2, atomic.EnergyCost.Offset);
        });
    }
    sealed class JobTestPower : PowerModel { }
}
