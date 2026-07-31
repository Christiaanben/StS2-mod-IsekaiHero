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

public sealed class BeginnerMagic() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new ExtraDamageVar(4m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Beginner Magic",
        "# Deal !Damage! damage. Exploit (you played a Skill this turn): deal !ExtraDamage! more damage.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if (IsConditionalEffectActive(PlayedSkillThisTurn()))
            damage += DynamicVars.ExtraDamage.BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars.ExtraDamage.UpgradeValueBy(2m);
    }

    private bool PlayedSkillThisTurn()
    {
        return CombatManager.Instance.History.CardPlaysFinished.Any(
            (CardPlayFinishedEntry entry) =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner &&
                entry.CardPlay.Card.Type == CardType.Skill);
    }
}
