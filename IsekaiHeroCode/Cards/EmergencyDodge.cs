using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class EmergencyDodge() : IsekaiHeroCard(0, CardType.Skill, CardRarity.Common, TargetType.None)
{
    private const int RequiredLevel = 3;

    public override bool HasConditionalEffects => true;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(3m, ValueProp.Move),
        new BlockVar("BonusBlock", 3m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Emergency Dodge",
        "# Gain !Block! Block. Exploit (Level 3+): gain !BonusBlock! more Block.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        if (!IsConditionalEffectActive(LevelPower.GetLevel(Owner.Creature) >= RequiredLevel))
            return;

        await CreatureCmd.GainBlock(Owner.Creature, (BlockVar)DynamicVars["BonusBlock"], play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1m);
        DynamicVars["BonusBlock"].UpgradeValueBy(1m);
    }
}
