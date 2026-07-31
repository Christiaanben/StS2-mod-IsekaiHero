using System;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class AntiBossArt() : IsekaiHeroCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override bool HasConditionalEffects => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
        new DamageVar("BonusDamage", 10m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Anti-Boss Art",
        "# Deal !Damage! damage. Exploit (target is an Elite or Boss): deal !BonusDamage! more damage.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        // Elite/Boss is an encounter classification in the base game; individual
        // MonsterModel instances deliberately do not carry a rarity flag.
        var roomType = play.Target.Monster?.CombatState.Encounter?.RoomType;
        var damage = DynamicVars.Damage.BaseValue;

        if (IsConditionalEffectActive(roomType is RoomType.Elite or RoomType.Boss))
            damage += DynamicVars["BonusDamage"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }
}
