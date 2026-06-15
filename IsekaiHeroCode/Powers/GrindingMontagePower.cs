using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public sealed class GrindingMontagePower : IsekaiHeroPower
{
    public override bool ShouldReceiveCombatHooks => true;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Grinding Montage",
        "At the start of your turn, upgrade {Amount} random Attack or Skill in your hand for this combat. If a chosen card is already upgraded, it costs 1 less this turn.",
        "At the start of your turn, upgrade {Amount} random Attacks or Skills in your hand for this combat. If a chosen card is already upgraded, it costs 1 less this turn.");

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner)
            return Task.CompletedTask;

        var candidates = PileType.Hand.GetPile(player).Cards
            .Where(card => card.Type is CardType.Attack or CardType.Skill)
            .ToList();

        var triggerCount = System.Math.Min(Amount, candidates.Count);
        if (triggerCount == 0)
            return Task.CompletedTask;

        Flash();

        for (var i = 0; i < triggerCount; i++)
        {
            var card = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (card == null)
                break;

            candidates.Remove(card);

            if (card.IsUpgradable)
                CardCmd.Upgrade(card);
            else if (!card.EnergyCost.CostsX)
                card.EnergyCost.AddThisTurn(-1);
        }

        return Task.CompletedTask;
    }
}
