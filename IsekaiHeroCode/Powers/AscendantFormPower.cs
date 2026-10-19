using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public sealed class AscendantFormPower : IsekaiHeroPower
{
    private sealed class Data { public int Used; }
    public override bool ShouldReceiveCombatHooks => true;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public int Remaining => Math.Max(0, Amount - GetInternalData<Data>().Used);
    public override int DisplayAmount => IsMutable ? Remaining : base.DisplayAmount;

    public override List<(string, string)> Localization => new PowerLoc(
        "Ascendant Form",
        "Your first {Amount} unmet Exploit conditions each turn count as met for free. Does not stack.",
        "Your first {Amount} unmet Exploit conditions each turn count as met for free. Does not stack.");

    protected override object InitInternalData() => new Data();

    internal bool TryUse()
    {
        if (Remaining == 0) return false;
        GetInternalData<Data>().Used++;
        InvokeDisplayAmountChanged();
        Flash();
        return true;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner)
        {
            GetInternalData<Data>().Used = 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
}
