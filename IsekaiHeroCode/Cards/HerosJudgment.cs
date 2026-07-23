using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class HerosJudgment() : IsekaiHeroCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    private const int RequiredLevel = 7;

    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move),
        new DamageVar("JudgmentDamage", 32m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Hero's Judgment",
        "# Deal !Damage! damage. Exploit (Level 7+): deal !JudgmentDamage! instead.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;

        if (IsConditionalEffectActive(LevelPower.GetLevel(Owner.Creature) >= RequiredLevel))
            damage = DynamicVars["JudgmentDamage"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["JudgmentDamage"].UpgradeValueBy(8m);
    }
}
