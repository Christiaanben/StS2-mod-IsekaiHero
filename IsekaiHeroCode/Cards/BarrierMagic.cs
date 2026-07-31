using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class BarrierMagic() : IsekaiHeroCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    private const int RequiredLevel = 4;

    public override bool HasConditionalEffects => true;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(13m, ValueProp.Move),
        new BlockVar("BonusBlock", 5m, ValueProp.Move)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "Barrier Magic",
        "# Gain !Block! Block. Exploit (Level 4+): gain !BonusBlock! more Block.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        if (!IsConditionalEffectActive(LevelPower.GetLevel(Owner.Creature) >= RequiredLevel))
            return;

        await CreatureCmd.GainBlock(Owner.Creature, (BlockVar)DynamicVars["BonusBlock"], play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["BonusBlock"].UpgradeValueBy(1m);
    }
}
