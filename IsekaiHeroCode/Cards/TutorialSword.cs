using System;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class TutorialSword() : IsekaiHeroCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DamageVar("JobDamage", 4m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Tutorial Sword",
        "# Deal !Damage! damage. If you have a Job, deal !JobDamage! more damage.");

    // public override string CustomPortraitPath => "tutorialsword.png".BigCardImagePath();
    // public override string PortraitPath => "tutorialsword.png".CardImagePath();
    // public override string BetaPortraitPath => "tutorialsword.png".CardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var damage = DynamicVars.Damage.BaseValue;
        if (HasJob())
            damage += DynamicVars["JobDamage"].BaseValue;

        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["JobDamage"].UpgradeValueBy(2m);
    }

    private bool HasJob()
    {
        return Owner.Creature.Powers.Any(power => power.GetType().Name.StartsWith("Job", StringComparison.Ordinal));
    }
}
