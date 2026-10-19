using System.Runtime.CompilerServices;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public sealed class ReturnByDeathPower : IsekaiHeroPower
{
    // Creature + combat identity prevents removal/reapplication from rearming a save.
    private static readonly ConditionalWeakTable<Creature, RescueState> Rescues = new();

    private sealed class RescueState(object? combat)
    {
        public object? Combat = combat;
        public bool Spent;
        public int Heal = 15;
        public int Exp = 8;
        public CardModel? Source;
    }

    public override bool ShouldReceiveCombatHooks => true;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [new HealVar(15m), new DynamicVar("Exp", 8m)];

    public override List<(string, string)> Localization => new PowerLoc(
        "Return by Death",
        "Once per combat, if you would die, heal {Heal} HP and gain {Exp} EXP instead. All copies share one rescue. Playing another copy cannot restore a spent rescue.",
        "Once per combat, if you would die, heal {Heal} HP and gain {Exp} EXP instead. All copies share one rescue. Playing another copy cannot restore a spent rescue.");

    private RescueState State
    {
        get
        {
            var state = Rescues.GetValue(Owner, creature => new RescueState(creature.CombatState));
            if (!ReferenceEquals(state.Combat, Owner.CombatState))
            {
                Rescues.Remove(Owner);
                state = Rescues.GetValue(Owner, creature => new RescueState(creature.CombatState));
            }
            return state;
        }
    }

    public void Configure(int heal, int exp, CardModel source)
    {
        var state = State;
        if (!state.Spent && heal >= state.Heal)
        {
            state.Heal = heal;
            state.Exp = exp;
            state.Source = source;
        }
        DynamicVars.Heal.BaseValue = state.Heal;
        DynamicVars["Exp"].BaseValue = state.Exp;
    }

    // The early prevention stage runs before late saves such as Lizard Tail.
    public override bool ShouldDie(Creature creature) =>
        creature != Owner || Owner.CombatState == null || State.Spent;

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || State.Spent)
            return;

        var state = State;
        state.Spent = true; // Reserve before HP/EXP reactions can re-enter death handling.
        Flash();
        await CreatureCmd.Heal(Owner, state.Heal);
        await LevelPower.GainExp(new BlockingPlayerChoiceContext(), Owner, state.Exp, state.Source);
    }
}
