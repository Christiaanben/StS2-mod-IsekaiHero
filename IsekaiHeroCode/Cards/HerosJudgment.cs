using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class HerosJudgment() : IsekaiHeroCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    private decimal _growth;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move),
        new DynamicVar("Increase", 4m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Hero's Judgment",
        "# Deal !Damage! damage.\nWhile in hand, Level Ups increase this card's damage by !Increase! this combat.");

    // Called once per actual Level Up, before relic reactions can draw or move cards.
    public void GainLevelDamage()
    {
        AssertMutable();
        _growth += DynamicVars["Increase"].BaseValue;
        DynamicVars.Damage.BaseValue += DynamicVars["Increase"].BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars["Increase"].UpgradeValueBy(2m);

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += _growth;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        DynamicVars.Damage.BaseValue -= _growth;
        _growth = 0;
        return base.AfterCombatEnd(room);
    }
}
