using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public sealed class ReturnByDeathPower : IsekaiHeroPower
{
    private static readonly ConditionalWeakTable<ReturnByDeathPower, ReturnByDeathState> SavedStates = new();

    public override bool ShouldReceiveCombatHooks => true;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Return by Death",
        "At the start of your next turn, return to your saved HP, Block, and powers. Lose all Energy.",
        "At the start of your next turn, return to your saved HP, Block, and powers. Lose all Energy.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner)
            return;

        if (!SavedStates.TryGetValue(this, out var state))
        {
            await PowerCmd.Remove(this);
            return;
        }

        var powersToRemove = Owner.Powers
            .Where(power => power != this)
            .ToArray();

        foreach (var power in powersToRemove)
            await PowerCmd.Remove(power);

        foreach (var snapshot in state.SavedPowers)
        {
            await PowerCmd.Apply(
                snapshot.Power.ToMutable(snapshot.Amount),
                Owner,
                snapshot.Amount,
                Owner,
                state.SourceCard,
                false);
        }

        var targetHp = Math.Min(state.SavedHp, Owner.MaxHp);
        if (Owner.CurrentHp != targetHp)
            await CreatureCmd.SetCurrentHp(Owner, targetHp);

        var blockDelta = state.SavedBlock - Owner.Block;
        if (blockDelta > 0)
            Owner.GainBlockInternal(blockDelta);
        else if (blockDelta < 0)
            Owner.LoseBlockInternal(-blockDelta);

        var playerCombatState = player.PlayerCombatState;
        if (playerCombatState != null)
            playerCombatState.LoseEnergy(playerCombatState.Energy);

        SavedStates.Remove(this);
        await PowerCmd.Remove(this);
    }

    public static void SaveState(
        ReturnByDeathPower power,
        int savedHp,
        int savedBlock,
        IReadOnlyList<PowerSnapshot> savedPowers,
        CardModel sourceCard)
    {
        SavedStates.Remove(power);
        SavedStates.Add(power, new ReturnByDeathState(savedHp, savedBlock, savedPowers, sourceCard));
    }

    public static PowerSnapshot Snapshot(PowerModel power)
    {
        return new PowerSnapshot(power, power.Amount);
    }

    public sealed record PowerSnapshot(PowerModel Power, int Amount);

    private sealed record ReturnByDeathState(
        int SavedHp,
        int SavedBlock,
        IReadOnlyList<PowerSnapshot> SavedPowers,
        CardModel SourceCard);
}
