#pragma warning disable CS9113 // Boundary doubles intentionally ignore presentation-only arguments.
// Minimal command-recording game boundary for headless behavioral tests.
// Production relics, card condition resolution, and EXP engine are linked unchanged.
namespace BaseLib.Abstracts
{
    public class RelicLoc(string name, string description, string flavor) : List<(string, string)> { }
    public class PowerLoc : List<(string, string)>
    {
        public PowerLoc(string name, string description, string smart, params (string, string)[] extra) { }
    }
    public abstract class CustomCardModel(int cost, MegaCrit.Sts2.Core.Entities.Cards.CardType type,
        MegaCrit.Sts2.Core.Entities.Cards.CardRarity rarity, MegaCrit.Sts2.Core.Entities.Cards.TargetType target)
        : MegaCrit.Sts2.Core.Models.CardModel
    {
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
    public enum CardType { Skill }
    public enum CardRarity { Common }
    public enum TargetType { None }
    public record CardPlay(MegaCrit.Sts2.Core.Models.CardModel Card);
}
namespace MegaCrit.Sts2.Core.Entities.Powers
{
    public enum PowerType { Buff }
    public enum PowerStackType { Counter }
}
namespace MegaCrit.Sts2.Core.GameActions.Multiplayer { public class PlayerChoiceContext { } }
namespace MegaCrit.Sts2.Core.ValueProps { public enum ValueProp { Unpowered } }
namespace MegaCrit.Sts2.Core.Combat
{
    public enum CombatSide { Player, Enemy }
    public interface ICombatState { int RoundNumber { get; } }
    public class CombatState : ICombatState
    {
        public int RoundNumber { get; set; } = 1;
        public List<MegaCrit.Sts2.Core.Entities.Creatures.Creature> Enemies { get; } = [];
        public IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> HittableEnemies => Enemies.Where(x => !x.IsDead);
    }
    public class CombatManager
    {
        public static CombatManager Instance { get; } = new();
        public bool IsInProgress { get; set; } = true;
    }
}
namespace MegaCrit.Sts2.Core.Entities.Players
{
    public class Player
    {
        public Player() { Creature = new() { Player = this }; }
        public Creatures.Creature Creature { get; }
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
    public class Creature
    {
        public Players.Player? Player { get; init; }
        public List<MegaCrit.Sts2.Core.Models.PowerModel> Powers { get; } = [];
        public MegaCrit.Sts2.Core.Combat.CombatState? CombatState { get; set; }
        public MegaCrit.Sts2.Core.Combat.CombatSide Side { get; set; }
        public bool IsDead { get; set; }
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
    }
    public class PowerModel : Model
    {
        private object? _data;
        public Entities.Creatures.Creature Owner { get; set; } = null!;
        public int Amount { get; set; }
        public virtual Entities.Powers.PowerType Type => default;
        public virtual Entities.Powers.PowerStackType StackType => default;
        protected virtual object InitInternalData() => new object();
        protected T GetInternalData<T>() => (T)(_data ??= InitInternalData());
    }
}
namespace MegaCrit.Sts2.Core.Models.Powers { public class VigorPower : PowerModel { } }
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
    public interface IHoverTip { }
    public class HoverTip(Localization.LocString title, Localization.LocString description) : IHoverTip { }
    public static class HoverTipFactory { public static IHoverTip FromPower<T>() => new HoverTip(new("", ""), new("", "")); }
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
        public static Task Apply<T>(PlayerChoiceContext context, Creature target, int amount, Creature applier,
            CardModel? source, bool silent = false) where T : PowerModel, new()
        {
            Events.Add(typeof(T).Name);
            var power = target.Powers.OfType<T>().FirstOrDefault();
            if (power == null) { power = new T { Owner = target }; target.Powers.Add(power); }
            power.Amount += amount;
            return Task.CompletedTask;
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
