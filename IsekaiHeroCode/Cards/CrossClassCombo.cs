using System;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class CrossClassCombo() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Cross-Class Combo",
        "# Deal !Damage! damage once for each different card type you played this turn before this card. {IfUpgraded:show:Count this card too.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var cardTypes = CombatManager.Instance.History.CardPlaysFinished
            .Where((CardPlayFinishedEntry entry) =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner)
            .Select(entry => entry.CardPlay.Card.Type)
            .ToHashSet();

        if (IsUpgraded)
            cardTypes.Add(Type);

        if (cardTypes.Count == 0)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(cardTypes.Count)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }
}
