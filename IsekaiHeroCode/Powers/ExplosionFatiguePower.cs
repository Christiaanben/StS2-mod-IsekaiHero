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

/// <summary>
/// Applies EXPLOSION!'s recovery turn after the player's next hand is drawn.
/// A prohibitive per-turn cost preserves normal card behaviour and clears when
/// the turn ends without permanently modifying a card's base cost.
/// </summary>
public sealed class ExplosionFatiguePower : IsekaiHeroPower
{
    private const int AttackCostWhileExhausted = 99;

    public override bool ShouldReceiveCombatHooks => true;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Explosion Fatigue",
        "At the start of your next turn, Attacks in your hand cost 99 this turn.",
        "At the start of your next turn, Attacks in your hand cost 99 this turn.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner)
            return;

        var attackCards = PileType.Hand.GetPile(player).Cards
            .Where(card => card.Type == CardType.Attack)
            .ToArray();

        foreach (var card in attackCards)
            card.EnergyCost.AddThisTurn(AttackCostWhileExhausted);

        Flash();
        await PowerCmd.Remove(this);
    }
}
