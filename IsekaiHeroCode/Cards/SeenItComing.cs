using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class SeenItComing() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool HasConditionalEffects => true;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6m, ValueProp.Move),
        new PowerVar<WeakPower>(1m)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Seen it Coming",
        "# Gain !Block! Block. If the enemy intends to attack, apply !WeakPower! Weak.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await CommonActions.CardBlock(this, play);

        if (!IsConditionalEffectActive(play.Target.Monster?.IntendsToAttack == true))
            return;

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            play.Target,
            DynamicVars.Weak.IntValue,
            Owner.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
