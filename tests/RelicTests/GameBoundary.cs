#pragma warning disable CS9113 // Boundary doubles intentionally ignore presentation-only arguments.
// Minimal command-recording game boundary for headless behavioral tests.
// Production relics, card condition resolution, and EXP engine are linked unchanged.
namespace BaseLib.Abstracts
{
    public class CardLoc(string name, string description, params (string, string)[] extra) : List<(string, string)> { }
    public class RelicLoc(string name, string description, string flavor) : List<(string, string)> { }
    public class PowerLoc : List<(string, string)>
    {
        public PowerLoc(string name, string description, string smart, params (string, string)[] extra) { }
    }
    public abstract class CustomCardModel(int cost, MegaCrit.Sts2.Core.Entities.Cards.CardType type,
        MegaCrit.Sts2.Core.Entities.Cards.CardRarity rarity, MegaCrit.Sts2.Core.Entities.Cards.TargetType target)
        : MegaCrit.Sts2.Core.Models.CardModel
    {
        public override int BaseCost => cost;
        public override MegaCrit.Sts2.Core.Entities.Cards.CardType Type => type;
        public virtual string CustomPortraitPath => "";
        public virtual string PortraitPath => "";
        public virtual string BetaPortraitPath => "";
    }
}
namespace BaseLib.Extensions
{
    public static class Strings { public static string RemovePrefix(this string s) => s; }
}
namespace BaseLib.Utils
{
    [AttributeUsage(AttributeTargets.Class)] public class PoolAttribute(Type type) : Attribute { }
}
namespace IsekaiHero.IsekaiHeroCode.Extensions
{
    public static class Strings
    {
        public static string BigCardImagePath(this string s) => s;
        public static string CardImagePath(this string s) => s;
    }
}
namespace IsekaiHero.IsekaiHeroCode.Character { public class IsekaiHeroCardPool { } }
namespace IsekaiHero.IsekaiHeroCode.Cards { public static class IsekaiHeroKeywords { public const int Override = 1; } }
namespace MegaCrit.Sts2.Core.Entities.Relics
{
    public enum RelicRarity { Common, Uncommon, Rare }
    public enum RelicStatus { Normal, Disabled }
}
namespace MegaCrit.Sts2.Core.Entities.Cards
{
    public enum CardType { Skill, Attack, Power }
    public enum CardRarity { Basic, Common, Uncommon, Rare }
    public enum TargetType { None, AnyEnemy, AllEnemies, Self }
    public enum CardTag { Strike }
    public enum CardKeyword { Retain, Exhaust }
    public enum PileType { Hand, Discard, Draw, Exhaust }
    public class CardPile
    {
        public List<MegaCrit.Sts2.Core.Models.CardModel> Cards { get; } = [];
        public static CardPile? Get(PileType type, MegaCrit.Sts2.Core.Entities.Players.Player player) => player.Piles[type];
    }
    public record CardPlay(MegaCrit.Sts2.Core.Models.CardModel Card, MegaCrit.Sts2.Core.Entities.Creatures.Creature? Target = null)
    { public int PlayIndex { get; init; } }
}
namespace MegaCrit.Sts2.Core.Entities.Powers
{
    public enum PowerType { Buff, Debuff }
    public enum PowerStackType { Counter, Single }
}
namespace MegaCrit.Sts2.Core.GameActions.Multiplayer { public class PlayerChoiceContext { } public class BlockingPlayerChoiceContext : PlayerChoiceContext { } }
namespace MegaCrit.Sts2.Core.ValueProps { public enum ValueProp { Unpowered, Move } }
namespace MegaCrit.Sts2.Core.Combat
{
    public enum CombatSide { Player, Enemy }
    public interface ICombatState { int RoundNumber { get; } }
    public class CombatState : ICombatState
    {
        public int RoundNumber { get; set; } = 1;
        public CombatSide CurrentSide { get; set; }
        public Encounter? Encounter { get; set; }
        public List<MegaCrit.Sts2.Core.Entities.Creatures.Creature> Enemies { get; } = [];
        public IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> HittableEnemies => Enemies.Where(x => !x.IsDead);
    }
    public class CombatManager
    {
        public static CombatManager Instance { get; } = new();
        public HistoryData History { get; } = new();
        public bool IsInProgress { get; set; } = true;
        public bool IsEnding { get; set; }
        public bool IsOverOrEnding => !IsInProgress || IsEnding;
    }
}
namespace MegaCrit.Sts2.Core.Entities.Players
{
    public class Player
    {
        public Player() { Creature = new() { Player = this }; }
        public Creatures.Creature Creature { get; }
        public Dictionary<Entities.Cards.PileType, Entities.Cards.CardPile> Piles { get; } = Enum.GetValues<Entities.Cards.PileType>().ToDictionary(x => x, _ => new Entities.Cards.CardPile());
        public int Gold;
        public decimal Energy;
        public List<IsekaiHero.IsekaiHeroCode.Relics.IsekaiHeroRelic> Relics { get; } = [];
        public RunState RunState { get; } = new();
    }
    public class RunState { public RngSet Rng { get; } = new(); }
    public class RngSet { public TestRng CombatTargets { get; } = new(); }
    public class TestRng
    {
        public int Calls;
        public int NextIndex;
        public T? NextItem<T>(IEnumerable<T> items)
        {
            Calls++;
            return items.ElementAtOrDefault(NextIndex);
        }
    }
}
namespace MegaCrit.Sts2.Core.Entities.Creatures
{
    public class Monster { public bool IntendsToAttack; public MegaCrit.Sts2.Core.Combat.CombatState CombatState { get; set; } = new(); }
    public class Creature
    {
        public Players.Player? Player { get; init; }
        public List<MegaCrit.Sts2.Core.Models.PowerModel> Powers { get; } = [];
        public MegaCrit.Sts2.Core.Combat.CombatState? CombatState { get; set; }
        public MegaCrit.Sts2.Core.Combat.CombatSide Side { get; set; }
        public decimal Block;
        public int CurrentHp = 50;
        public int MaxHp = 70;
        public Monster? Monster = new();
        public bool IsDead { get; set; }
        public bool IsEnemy => Side == MegaCrit.Sts2.Core.Combat.CombatSide.Enemy;
    }
}
namespace MegaCrit.Sts2.Core.Models
{
    public class ModelId { public string Entry => "TEST"; }
    public abstract class Model
    {
        public ModelId Id { get; } = new();
        public bool IsMutable => true;
        public int Flashes { get; private set; }
        public int DisplayChanges { get; private set; }
        public void AssertMutable() { }
        public virtual Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room) => Task.CompletedTask;
        public virtual bool ShouldReceiveCombatHooks => true;
        public void Flash() => Flashes++;
        public void InvokeDisplayAmountChanged() => DisplayChanges++;
        public virtual List<(string, string)> Localization => [];
        protected virtual IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> ExtraHoverTips => [];
        public virtual Task BeforeCardPlayed(Entities.Cards.CardPlay play) => Task.CompletedTask;
        public virtual Task AfterCardPlayed(GameActions.Multiplayer.PlayerChoiceContext context, Entities.Cards.CardPlay play) => Task.CompletedTask;
    }
    public class CardModel : Model
    {
        public Entities.Players.Player Owner { get; set; } = null!;
        public Combat.CombatState? CombatState => Owner.Creature.CombatState;
        public virtual int BaseCost => 0;
        public virtual Entities.Cards.CardType Type => default;
        public string SelectionScreenPrompt => "Select a card";
        public bool IsUpgraded { get; private set; }
        public virtual bool GainsBlock => false;
        protected MegaCrit.Sts2.Core.HoverTips.IHoverTip EnergyHoverTip => MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.ForEnergy(this);
        public virtual IEnumerable<Entities.Cards.CardKeyword> CanonicalKeywords => [];
        public HashSet<Entities.Cards.CardKeyword> Keywords { get; } = [];
        protected virtual HashSet<Entities.Cards.CardTag> CanonicalTags => [];
        public HashSet<Entities.Cards.CardTag> Tags => CanonicalTags;
        public void AddKeyword(Entities.Cards.CardKeyword keyword) => Keywords.Add(keyword);
        protected virtual IEnumerable<Localization.DynamicVars.DynamicVar> CanonicalVars => [];
        private Localization.DynamicVars.Vars? _vars;
        public Localization.DynamicVars.Vars DynamicVars => _vars ??= new(CanonicalVars);
        public Entities.Cards.CardEnergyCost EnergyCost { get; } = new();
        protected virtual Task OnPlay(GameActions.Multiplayer.PlayerChoiceContext context, Entities.Cards.CardPlay play) => Task.CompletedTask;
        protected virtual void OnUpgrade() { }
        protected virtual void AfterDowngraded() { }
        public void Upgrade() { _ = DynamicVars; IsUpgraded = true; OnUpgrade(); }
        public Task Play(GameActions.Multiplayer.PlayerChoiceContext context, Entities.Cards.CardPlay play) => OnPlay(context, play);
    }
    public class PowerModel : Model
    {
        private object? _data;
        public Entities.Creatures.Creature Owner { get; set; } = null!;
        public int Amount { get; set; }
        public virtual int DisplayAmount => Amount;
        protected virtual IEnumerable<Localization.DynamicVars.DynamicVar> CanonicalVars => [];
        private Localization.DynamicVars.Vars? _vars;
        public Localization.DynamicVars.Vars DynamicVars => _vars ??= new(CanonicalVars);
        public virtual decimal ModifyMaxEnergy(Entities.Players.Player player, decimal amount) => amount;
        public virtual bool ShouldDie(Entities.Creatures.Creature creature) => true;
        public virtual bool ShouldOwnerDeathTriggerFatal() => true;
        public virtual Task AfterPreventingDeath(Entities.Creatures.Creature creature) => Task.CompletedTask;
        public virtual Task AfterPlayerTurnStart(GameActions.Multiplayer.PlayerChoiceContext context, Entities.Players.Player player) => Task.CompletedTask;
        public virtual Task AfterPowerAmountChanged(GameActions.Multiplayer.PlayerChoiceContext context, PowerModel power, decimal amount, Entities.Creatures.Creature? applier, CardModel? source) => Task.CompletedTask;
        public virtual Entities.Powers.PowerType GetTypeForAmount(decimal amount) => Type;
        public virtual Entities.Powers.PowerType Type => default;
        public virtual Entities.Powers.PowerStackType StackType => default;
        protected virtual object InitInternalData() => new object();
        protected T GetInternalData<T>() => (T)(_data ??= InitInternalData());
    }
}
namespace MegaCrit.Sts2.Core.Models.Powers { public class VigorPower : PowerModel { } public class StrengthPower : PowerModel { } public class DexterityPower : PowerModel { } public class WeakPower : PowerModel { public override Entities.Powers.PowerType Type => Entities.Powers.PowerType.Debuff; } public class VulnerablePower : PowerModel { public override Entities.Powers.PowerType Type => Entities.Powers.PowerType.Debuff; } }
namespace IsekaiHero.IsekaiHeroCode.Powers
{
    public abstract class IsekaiHeroPower : MegaCrit.Sts2.Core.Models.PowerModel { }
}
namespace IsekaiHero.IsekaiHeroCode.Relics
{
    public abstract class IsekaiHeroRelic : MegaCrit.Sts2.Core.Models.Model
    {
        public MegaCrit.Sts2.Core.Entities.Players.Player Owner { get; set; } = null!;
        public abstract MegaCrit.Sts2.Core.Entities.Relics.RelicRarity Rarity { get; }
        public MegaCrit.Sts2.Core.Entities.Relics.RelicStatus Status { get; set; }
        public virtual Task BeforeCombatStart() => Task.CompletedTask;
        public virtual Task BeforeSideTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext context,
            MegaCrit.Sts2.Core.Combat.CombatSide side, IReadOnlyList<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants, MegaCrit.Sts2.Core.Combat.ICombatState combat) => Task.CompletedTask;
    }
}
namespace MegaCrit.Sts2.Core.HoverTips
{
    public enum StaticHoverTip { Fatal }
    public interface IHoverTip { }
    public class HoverTip(Localization.LocString title, Localization.LocString description) : IHoverTip { }
    public static class HoverTipFactory { public static IHoverTip Static(StaticHoverTip tip) => new HoverTip(new("", ""), new("", "")); public static IHoverTip ForEnergy(object model) => new HoverTip(new("", ""), new("", "")); public static IHoverTip FromKeyword(int keyword) => new HoverTip(new("", ""), new("", "")); public static IHoverTip FromPower<T>() => new HoverTip(new("", ""), new("", "")); }
}
namespace MegaCrit.Sts2.Core.Localization
{
    public class LocString(string table, string key) { public void Add(string name, object value) { } }
}
namespace MegaCrit.Sts2.Core.Commands
{
    using Entities.Creatures;
    using GameActions.Multiplayer;
    using Models;
    public static class PowerCmd
    {
        public static readonly List<string> Events = [];
        public static async Task Apply<T>(PlayerChoiceContext context, IEnumerable<Creature> targets, decimal amount, Creature applier,
            CardModel? source, bool silent = false) where T : PowerModel, new()
        {
            foreach (var target in targets) await Apply<T>(context, target, amount, applier, source, silent);
        }
        public static async Task<T?> Apply<T>(PlayerChoiceContext context, Creature target, decimal amount, Creature applier,
            CardModel? source, bool silent = false) where T : PowerModel, new()
        {
            Events.Add(typeof(T).Name);
            var power = target.Powers.OfType<T>().FirstOrDefault();
            if (power == null) { power = new T { Owner = target }; target.Powers.Add(power); }
            power.Amount += (int)amount;
            foreach (var listener in applier.Powers.ToArray())
                await listener.AfterPowerAmountChanged(context, power, amount, applier, source);
            return power;
        }
        public static Task Decrement(PowerModel power)
        {
            power.Amount--;
            if (power.Amount == 0) power.Owner.Powers.Remove(power);
            return Task.CompletedTask;
        }
    }
    public static class CardCmd { public static void ApplyKeyword(CardModel card, int keyword) { } }
    public static class CreatureCmd
    {
        public record Hit(Creature Target, decimal Damage, ValueProps.ValueProp Props, Creature Dealer);
        public static readonly List<Hit> Hits = [];
        public static Func<Hit, Task>? OnDamage;
        public static Func<Task>? OnHeal;
        public static async Task Heal(Creature creature, decimal amount)
        {
            creature.CurrentHp = Math.Min(creature.MaxHp, creature.CurrentHp + (int)amount);
            PowerCmd.Events.Add("Heal");
            if (OnHeal != null) await OnHeal();
        }
        public static Task GainBlock(Creature creature, Localization.DynamicVars.DynamicVar block, Entities.Cards.CardPlay play)
        {
            creature.Block += block.BaseValue;
            PowerCmd.Events.Add("Block");
            return Task.CompletedTask;
        }
        public static Task GainBlock(Creature creature, decimal amount, ValueProps.ValueProp props, Models.CardModel? source, bool fast = false)
        {
            creature.Block += amount;
            PowerCmd.Events.Add("Block");
            return Task.CompletedTask;
        }
        public static async Task Damage(PlayerChoiceContext context, IEnumerable<Creature> targets, decimal amount,
            ValueProps.ValueProp props, Creature dealer)
        {
            foreach (var target in targets)
            {
                var hit = new Hit(target, amount, props, dealer);
                Hits.Add(hit);
                PowerCmd.Events.Add("Damage");
                if (OnDamage != null) await OnDamage(hit);
            }
        }
    }
}

namespace MegaCrit.Sts2.Core.Rooms { public class CombatRoom { } public enum RoomType { Monster, Elite, Boss } }
namespace MegaCrit.Sts2.Core.Combat { public class Encounter { public Rooms.RoomType RoomType { get; set; } } }
namespace MegaCrit.Sts2.Core.Localization.DynamicVars
{
    public class DynamicVar(string name, decimal value)
    {
        public string Name => name;
        public decimal BaseValue { get; set; } = value;
        public int IntValue => (int)BaseValue;
        public void UpgradeValueBy(decimal value) => BaseValue += value;
    }
    public class DamageVar : DynamicVar { public DamageVar(decimal value, ValueProps.ValueProp props) : base("Damage", value) { } public DamageVar(string name, decimal value, ValueProps.ValueProp props) : base(name, value) { } }
    public class BlockVar : DynamicVar { public BlockVar(decimal value, ValueProps.ValueProp props) : base("Block", value) { } public BlockVar(string name, decimal value, ValueProps.ValueProp props) : base(name, value) { } }
    public class ExtraDamageVar(decimal value) : DynamicVar("ExtraDamage", value) { }
    public class GoldVar(decimal value) : DynamicVar("Gold", value) { }
    public class HealVar(decimal value) : DynamicVar("Heal", value) { }
    public class CardsVar(decimal value) : DynamicVar("Cards", value) { }
    public class RepeatVar(decimal value) : DynamicVar("Repeat", value) { }
    public class EnergyVar(decimal value) : DynamicVar("Energy", value) { }
    public class PowerVar<T>(decimal value) : DynamicVar(typeof(T).Name, value) { }
    public class Vars(IEnumerable<DynamicVar> vars)
    {
        private readonly Dictionary<string, DynamicVar> _vars = vars.ToDictionary(v => v.Name);
        public DynamicVar this[string name] => _vars[name];
        public DynamicVar Damage => this["Damage"];
        public DynamicVar Block => this["Block"];
        public DynamicVar Gold => this["Gold"];
        public DynamicVar ExtraDamage => this["ExtraDamage"];
        public DynamicVar Heal => this["Heal"];
        public DynamicVar Cards => this["Cards"];
        public DynamicVar Repeat => this["Repeat"];
        public DynamicVar Energy => this["Energy"];
        public DynamicVar Weak => this["WeakPower"];
        public DynamicVar Vulnerable => this["VulnerablePower"];
    }
}
namespace MegaCrit.Sts2.Core.Combat
{
    public class HistoryData { public List<History.Entries.CardPlayFinishedEntry> CardPlaysFinished { get; } = []; }
    public class HistoryEntry(Entities.Cards.CardPlay play, CombatState combat)
    {
        public Entities.Cards.CardPlay CardPlay => play;
        public CombatState Combat => combat;
        public int Round { get; } = combat.RoundNumber;
    }
}
namespace MegaCrit.Sts2.Core.Combat.History.Entries
{
    public class CardPlayFinishedEntry(Entities.Cards.CardPlay play, CombatState combat) : Combat.HistoryEntry(play, combat) { }
    public static class HistoryExtensions
    {
        public static bool HappenedThisTurn(this Combat.HistoryEntry entry, Combat.CombatState? combat) =>
            entry.Combat == combat && entry.Round == combat?.RoundNumber;
    }
}
namespace BaseLib.Utils
{
    public static class CommonActions
    {
        public static Task CardBlock(MegaCrit.Sts2.Core.Models.CardModel card, MegaCrit.Sts2.Core.Entities.Cards.CardPlay play) =>
            MegaCrit.Sts2.Core.Commands.CreatureCmd.GainBlock(card.Owner.Creature, card.DynamicVars.Block, play);
    }
}
namespace MegaCrit.Sts2.Core.Entities.Cards
{
    // Models the native cost expiration contract, not game rendering or hook dispatch.
    public class CardEnergyCost
    {
        private readonly List<(int Value, bool UntilPlayed)> _offsets = [];
        public bool CostsX => false;
        public int Offset => _offsets.Sum(x => x.Value);
        public void AddUntilPlayed(int value) => _offsets.Add((value, true));
        public void AddThisCombat(int value) => _offsets.Add((value, false));
        public void AfterCardPlayedCleanup() => _offsets.RemoveAll(x => x.UntilPlayed);
    }
}
namespace MegaCrit.Sts2.Core.Commands
{
    using Entities.Cards;
    using Entities.Creatures;
    using Entities.Players;
    using GameActions.Multiplayer;
    using Models;
    public static class PlayerCmd
    {
        public static Task GainEnergy(decimal amount, Player player) { player.Energy += amount; PowerCmd.Events.Add("Energy"); return Task.CompletedTask; }
        public static List<Player> EndedTurns { get; } = [];
        public static void EndTurn(Player player, bool canBackOut) { EndedTurns.Add(player); PowerCmd.Events.Add("EndTurn"); }
        public static Task GainGold(decimal amount, Player player, bool silent) { player.Gold += (int)amount; return Task.CompletedTask; }
    }
    public static class CardPileCmd
    {
        public static Task Add(CardModel card, PileType pile)
        {
            foreach (var existing in card.Owner.Piles.Values) existing.Cards.Remove(card);
            card.Owner.Piles[pile].Cards.Add(card);
            return Task.CompletedTask;
        }
        public static int Drawn;
        public static Task Draw(PlayerChoiceContext context, decimal amount, Player player) { Drawn += (int)amount; return Task.CompletedTask; }
    }
    public static class DamageCmd
    {
        public static AttackBuilder Attack(decimal amount) => new(amount);
    }
    public class AttackBuilder(decimal amount)
    {
        public record Result(Creature Receiver, bool WasTargetKilled);
        public List<List<Result>> Results { get; } = [];
        private CardModel _card = null!;
        private Creature[] _targets = [];
        private int _hits = 1;
        public AttackBuilder FromCard(CardModel card) { _card = card; return this; }
        public AttackBuilder Targeting(Creature target) { _targets = [target]; return this; }
        public AttackBuilder TargetingAllOpponents(Combat.CombatState combat) { _targets = combat.HittableEnemies.ToArray(); return this; }
        public AttackBuilder WithHitFx(string? fx, string? other = null, string? sound = null) => this;
        public AttackBuilder WithNoAttackerAnim() => this;
        public AttackBuilder WithHitCount(int hits) { _hits = hits; return this; }
        public async Task<AttackBuilder> Execute(PlayerChoiceContext context)
        {
            for (var i = 0; i < _hits; i++)
            {
                await CreatureCmd.Damage(context, _targets, amount, ValueProps.ValueProp.Move, _card.Owner.Creature);
                Results.Add(_targets.Select(target => new Result(target, target.IsDead)).ToList());
            }
            return this;
        }
    }
}

namespace IsekaiHero.IsekaiHeroCode.Vfx
{
    public class AtomicVfx
    {
        public static readonly List<string> Events = [];
        public static Task<AtomicVfx?> Charge(MegaCrit.Sts2.Core.Entities.Creatures.Creature caster, bool replay)
        { Events.Add(replay ? "CompactCharge" : "Charge"); return Task.FromResult<AtomicVfx?>(new()); }
        public void Detonate() => Events.Add("Detonate");
    }
}

namespace MegaCrit.Sts2.Core.CardSelection
{
    public class CardSelectorPrefs(string prompt, int count) { }
}
namespace MegaCrit.Sts2.Core.Commands
{
    public static class CardSelectCmd
    {
        public static Task<IEnumerable<Models.CardModel>> FromHand(GameActions.Multiplayer.PlayerChoiceContext context,
            Entities.Players.Player player, CardSelection.CardSelectorPrefs prefs, object? filter, Models.CardModel source) =>
            Task.FromResult<IEnumerable<Models.CardModel>>(player.Piles[Entities.Cards.PileType.Hand].Cards.Take(1).ToArray());
        public static Task<IEnumerable<Models.CardModel>> FromSimpleGrid(GameActions.Multiplayer.PlayerChoiceContext context,
            IEnumerable<Models.CardModel> cards, Entities.Players.Player player, CardSelection.CardSelectorPrefs prefs) =>
            Task.FromResult<IEnumerable<Models.CardModel>>(cards.Take(1).ToArray());
    }
}
