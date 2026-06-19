using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public sealed class AlchemistPower : IsekaiHeroPower
{
    private sealed class Data
    {
        public bool TriggeredThisTurn;

        public readonly Dictionary<Creature, decimal> DamageBonuses = [];
    }

    public override bool ShouldReceiveCombatHooks => true;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Alchemist",
        "The first time each turn you apply a debuff, gain {Amount} Block and your next Attack against that enemy deals {Amount} more damage.",
        "The first time each turn you apply a debuff, gain {Amount} Block and your next Attack against that enemy deals {Amount} more damage.");

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
            GetInternalData<Data>().TriggeredThisTurn = false;

        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (Owner == null ||
            data.TriggeredThisTurn ||
            amount == 0m ||
            applier != Owner ||
            !power.Owner.IsEnemy ||
            power.GetTypeForAmount(amount) != PowerType.Debuff)
        {
            return;
        }

        data.TriggeredThisTurn = true;
        data.DamageBonuses.TryGetValue(power.Owner, out var existingBonus);
        data.DamageBonuses[power.Owner] = existingBonus + Amount;
        Flash();

        await CreatureCmd.GainBlock(
            Owner,
            Amount,
            ValueProp.Unpowered,
            null,
            fast: true);
    }

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (Owner == null ||
            dealer != Owner ||
            target == null ||
            cardSource?.Type != CardType.Attack ||
            !props.IsPoweredAttack() ||
            !data.DamageBonuses.TryGetValue(target, out var bonus))
        {
            return 0m;
        }

        return bonus;
    }

    public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        var data = GetInternalData<Data>();
        if (Owner != null &&
            command.Attacker == Owner &&
            command.ModelSource is CardModel { Type: CardType.Attack })
        {
            foreach (var target in command.Results
                         .SelectMany(results => results)
                         .Select(result => result.Receiver)
                         .Distinct()
                         .ToArray())
            {
                data.DamageBonuses.Remove(target);
            }
        }

        return Task.CompletedTask;
    }
}
