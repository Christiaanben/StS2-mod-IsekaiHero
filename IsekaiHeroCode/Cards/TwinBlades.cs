using System;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class TwinBlades() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    private const int RequiredLevel = 4;

    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(2)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Twin Blades",
        "# Deal !Damage! damage !Repeat! times. Exploit (Level 4+): deal !Damage! damage a third time.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var hitCount = DynamicVars.Repeat.IntValue;
        if (IsConditionalEffectActive(LevelPower.GetLevel(Owner.Creature) >= RequiredLevel))
            hitCount++;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
