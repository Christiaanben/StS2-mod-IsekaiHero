using System;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class TutorialStrike() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public override bool HasConditionalEffects => true;

    protected override bool IsNaturalConditionMet => HasJob() || PlayedPowerThisTurn();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new ExtraDamageVar(4m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Tutorial Strike",
        "# Deal !Damage! damage.\nIf you have a Job or played a Power this turn, deal !ExtraDamage! more.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if (IsConditionalEffectActive(IsNaturalConditionMet))
            damage += DynamicVars.ExtraDamage.BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }

    private bool PlayedPowerThisTurn() => CombatManager.Instance.History.CardPlaysFinished.Any(entry =>
        entry.HappenedThisTurn(CombatState) && entry.CardPlay.Card.Owner == Owner &&
        entry.CardPlay.Card.Type == CardType.Power);

    private bool HasJob()
    {
        return Owner.Creature.Powers.Any(power => power is AlchemistPower ||
            power.GetType().Name.StartsWith("Job", StringComparison.Ordinal));
    }
}
