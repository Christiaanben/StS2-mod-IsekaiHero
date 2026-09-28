using IsekaiHero.IsekaiHeroCode.Cards;
using IsekaiHero.IsekaiHeroCode.Powers;
using IsekaiHero.IsekaiHeroCode.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

var context = new PlayerChoiceContext();
var tests = new List<(string, Func<Task>)>();
void Test(string name, Func<Task> body) => tests.Add((name, body));
void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected {expected}; got {actual}");
}
Player Player()
{
    var player = new Player();
    player.Creature.CombatState = new CombatState();
    player.Creature.CombatState.Enemies.Add(new Creature { Side = CombatSide.Enemy });
    return player;
}
T Give<T>(Player player) where T : IsekaiHeroRelic, new()
{
    var relic = new T { Owner = player };
    player.Relics.Add(relic);
    return relic;
}
async Task Play(TestCard card, bool condition)
{
    var play = new CardPlay(card);
    await card.BeforeCardPlayed(play);
    card.LastResult = card.Check(condition);
    foreach (var exploit in card.Owner.Creature.Powers.OfType<ExploitPower>().ToArray())
        await exploit.AfterCardPlayed(context, play);
    await card.AfterCardPlayed(context, play);
}
Test("rarities match the design", () =>
{
    Equal(RelicRarity.Common, new BeginnersLuckCharm().Rarity);
    Equal(RelicRarity.Uncommon, new OpSmartphone().Rarity);
    Equal(RelicRarity.Rare, new ForbiddenWalkthrough().Rarity);
    return Task.CompletedTask;
});
Test("charm bypasses only the first unmet play", async () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    await Play(card, false); Equal(true, card.LastResult); Equal(true, card.DidTriggerConditionalEffectThisPlay());
    Equal(false, card.DidConsumeExploitThisPlay()); Equal(RelicStatus.Disabled, charm.Status);
    await Play(card, false); Equal(false, card.LastResult); Equal(1, charm.Flashes);
});
Test("natural conditions preserve luck", async () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    await Play(card, true); Equal(0, charm.Flashes);
    await Play(card, false); Equal(true, card.LastResult); Equal(1, charm.Flashes);
});
Test("Override precedes luck and stacks", async () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    await PowerCmd.Apply<ExploitPower>(context, p.Creature, 3, p.Creature, null);
    card.EnableConditionalEffectsForCombat(); await Play(card, false);
    Equal(true, card.LastResult); Equal(0, charm.Flashes); Equal(3, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
});
Test("luck precedes stacks; next replay consumes exactly one", async () =>
{
    var p = Player(); Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    await PowerCmd.Apply<ExploitPower>(context, p.Creature, 3, p.Creature, null);
    await Play(card, false); Equal(3, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
    await Play(card, false); Equal(true, card.LastResult); Equal(2, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
    Equal(true, card.DidConsumeExploitThisPlay());
});
Test("repeated condition checks in one play reuse luck", async () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    var play = new CardPlay(card); await card.BeforeCardPlayed(play);
    Equal(true, card.Check(false)); Equal(true, card.Check(false)); Equal(1, charm.Flashes);
    await card.AfterCardPlayed(context, play); Equal(false, card.Check(false));
});
Test("checking outside a play does not spend luck", () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    card.Check(false); card.Check(false); Equal(0, charm.Flashes);
    return Task.CompletedTask;
});
Test("charm resets next combat", async () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p); var card = new TestCard { Owner = p };
    await Play(card, false); await charm.BeforeCombatStart(); Equal(RelicStatus.Normal, charm.Status);
    p.Creature.CombatState = new CombatState(); await Play(card, false); Equal(true, card.LastResult); Equal(2, charm.Flashes);
});
Test("one player's charm cannot help another", async () =>
{
    var p = Player(); var charm = Give<BeginnersLuckCharm>(p);
    var card = new TestCard { Owner = Player() }; await Play(card, false);
    Equal(false, card.LastResult); Equal(0, charm.Flashes);
});
Test("walkthrough grants three before first player turn only", async () =>
{
    var p = Player(); var relic = Give<ForbiddenWalkthrough>(p); var combat = p.Creature.CombatState!;
    await relic.BeforeCombatStart(); await relic.BeforeSideTurnStart(context, CombatSide.Enemy, [], combat);
    Equal(0, p.Creature.Powers.Count);
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], combat);
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], combat);
    combat.RoundNumber = 2; await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], combat);
    Equal(3, p.Creature.Powers.OfType<ExploitPower>().Single().Amount); Equal(1, relic.Flashes);
});
Test("walkthrough resets and stacks with existing Exploit", async () =>
{
    var p = Player(); var relic = Give<ForbiddenWalkthrough>(p); var combat = p.Creature.CombatState!;
    await PowerCmd.Apply<ExploitPower>(context, p.Creature, 2, p.Creature, null);
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], combat);
    Equal(5, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
    p.Creature.Powers.Clear(); await relic.BeforeCombatStart();
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], combat);
    Equal(3, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
});
Test("walkthrough acquired mid-combat does not grant stacks", async () =>
{
    var p = Player(); var relic = Give<ForbiddenWalkthrough>(p); p.Creature.CombatState!.RoundNumber = 2;
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], p.Creature.CombatState);
    Equal(0, p.Creature.Powers.Count);
});
Test("smartphone ignores initial Level 1 and incomplete EXP", async () =>
{
    var p = Player(); Give<OpSmartphone>(p);
    await LevelPower.GainExp(context, p.Creature, 2, null);
    Equal(1, LevelPower.GetLevel(p.Creature)); Equal(0, CreatureCmd.Hits.Count);
    await LevelPower.GainExp(context, p.Creature, 1, null); Equal(0, CreatureCmd.Hits.Count);
});
Test("smartphone fires for every level, after Vigor", async () =>
{
    var p = Player(); var phone = Give<OpSmartphone>(p);
    await LevelPower.GainExp(context, p.Creature, 9, null);
    Equal(3, LevelPower.GetLevel(p.Creature)); Equal(2, CreatureCmd.Hits.Count); Equal(2, phone.Flashes);
    Equal(1, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
    Equal(4, p.Creature.Powers.OfType<VigorPower>().Single().Amount);
    Equal("LevelPower,LevelPower,VigorPower,Damage,LevelPower,VigorPower,Damage", string.Join(",", PowerCmd.Events));
    foreach (var hit in CreatureCmd.Hits)
    {
        Equal(5m, hit.Damage); Equal(ValueProp.Unpowered, hit.Props); Equal(p.Creature, hit.Dealer);
    }
});
Test("smartphone uses combat-target RNG and living targets", async () =>
{
    var p = Player(); Give<OpSmartphone>(p);
    var combat = p.Creature.CombatState!;
    combat.Enemies.Insert(0, new Creature { IsDead = true });
    var other = new Creature(); combat.Enemies.Add(other);
    p.RunState.Rng.CombatTargets.NextIndex = 1;
    await LevelPower.GainExp(context, p.Creature, 4, null);
    Equal(other, CreatureCmd.Hits.Single().Target); Equal(1, p.RunState.Rng.CombatTargets.Calls);
});
Test("no targets skips damage and RNG", async () =>
{
    var p = Player(); var phone = Give<OpSmartphone>(p); p.Creature.CombatState!.Enemies.Clear();
    await phone.AfterLevelUp(context); Equal(0, CreatureCmd.Hits.Count); Equal(0, p.RunState.Rng.CombatTargets.Calls);
});
Test("level cap banks EXP without additional smartphone hits", async () =>
{
    var p = Player(); Give<OpSmartphone>(p);
    await LevelPower.GainExp(context, p.Creature, 44, null);
    Equal(10, LevelPower.GetLevel(p.Creature)); Equal(9, CreatureCmd.Hits.Count);
    Equal(8, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
    await LevelPower.GainExp(context, p.Creature, 4, null); Equal(9, CreatureCmd.Hits.Count);
});
Test("kill EXP is queued, never recursively resolves reactions", async () =>
{
    var p = Player(); Give<OpSmartphone>(p); var inside = false; var nested = false;
    CreatureCmd.OnDamage = async hit =>
    {
        if (inside) nested = true;
        inside = true;
        if (CreatureCmd.Hits.Count == 1) await LevelPower.GainExp(context, p.Creature, 4, null);
        inside = false;
    };
    await LevelPower.GainExp(context, p.Creature, 4, null);
    Equal(false, nested); Equal(2, CreatureCmd.Hits.Count); Equal(3, LevelPower.GetLevel(p.Creature));
});
Test("combat ending on first hit stops further level chains", async () =>
{
    var p = Player(); Give<OpSmartphone>(p);
    CreatureCmd.OnDamage = hit => { CombatManager.Instance.IsInProgress = false; return Task.CompletedTask; };
    await LevelPower.GainExp(context, p.Creature, 12, null);
    Equal(1, CreatureCmd.Hits.Count); Equal(2, LevelPower.GetLevel(p.Creature));
    Equal(8, p.Creature.Powers.OfType<LevelPower>().Single().Experience);
});
Test("dead owner cannot trigger smartphone", async () =>
{
    var p = Player(); var phone = Give<OpSmartphone>(p); p.Creature.IsDead = true;
    await phone.AfterLevelUp(context); Equal(0, CreatureCmd.Hits.Count);
});
Test("allied level gains do not trigger the owner's smartphone", async () =>
{
    var p = Player(); Give<OpSmartphone>(p);
    await LevelPower.GainExp(context, Player().Creature, 4, null); Equal(0, CreatureCmd.Hits.Count);
});
Test("reaction failures release the processing guard", async () =>
{
    var p = Player(); Give<OpSmartphone>(p);
    CreatureCmd.OnDamage = hit => throw new InvalidOperationException("test failure");
    try { await LevelPower.GainExp(context, p.Creature, 4, null); } catch (InvalidOperationException) { }
    CreatureCmd.OnDamage = null;
    await LevelPower.GainExp(context, p.Creature, 4, null); Equal(3, LevelPower.GetLevel(p.Creature)); Equal(2, CreatureCmd.Hits.Count);
});
Test("walkthrough waits until its owner participates", async () =>
{
    var p = Player(); var relic = Give<ForbiddenWalkthrough>(p); var combat = p.Creature.CombatState!;
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [], combat);
    Equal(0, p.Creature.Powers.Count);
    await relic.BeforeSideTurnStart(context, CombatSide.Player, [p.Creature], combat);
    Equal(3, p.Creature.Powers.OfType<ExploitPower>().Single().Amount);
});
Test("smartphone does not target after combat has ended", async () =>
{
    var p = Player(); var phone = Give<OpSmartphone>(p);
    CombatManager.Instance.IsInProgress = false;
    await phone.AfterLevelUp(context);
    Equal(0, CreatureCmd.Hits.Count); Equal(0, p.RunState.Rng.CombatTargets.Calls);
});
var failed = 0;
foreach (var (name, body) in tests)
{
    CombatManager.Instance.IsInProgress = true;
    CreatureCmd.Hits.Clear(); CreatureCmd.OnDamage = null; PowerCmd.Events.Clear();
    try { await body(); Console.WriteLine($"PASS {name}"); }
    catch (Exception e) { failed++; Console.Error.WriteLine($"FAIL {name}: {e.Message}"); }
}
Console.WriteLine($"{tests.Count - failed}/{tests.Count} tests passed.");
return failed == 0 ? 0 : 1;

sealed class TestCard() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    public bool LastResult;
    public bool Check(bool condition) => IsConditionalEffectActive(condition);
}
