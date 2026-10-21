using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public sealed class AlchemistPower : IsekaiHeroPower
{
    private sealed class Data { public bool TriggeredThisTurn; }

    public override bool ShouldReceiveCombatHooks => true;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

    public override List<(string, string)> Localization => new PowerLoc(
        "Alchemist",
        "The first time each turn you apply a debuff to an enemy, gain {Amount} Block and {Amount} Vigor.",
        "The first time each turn you apply a debuff to an enemy, gain {Amount} Block and {Amount} Vigor.");

    protected override object InitInternalData() => new Data();

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == player.Creature)
            GetInternalData<Data>().TriggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (data.TriggeredThisTurn || amount == 0 || applier != Owner || !power.Owner.IsEnemy ||
            power.GetTypeForAmount(power.Amount) != PowerType.Debuff ||
            power.GetTypeForAmount(amount) != PowerType.Debuff ||
            power.GetTypeForAmount(power.Amount - amount) == PowerType.Debuff &&
            Math.Abs(power.Amount) <= Math.Abs(power.Amount - amount))
            return;

        // Reserve before commands dispatch more power hooks, including AoE applications.
        data.TriggeredThisTurn = true;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, cardSource);
    }
}
