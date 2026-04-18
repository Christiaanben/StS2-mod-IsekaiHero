using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class DefendIsekaiHero() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Basic, TargetType.None)
{
    public override bool GainsBlock => true;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    public override List<(string, string)> Localization => new CardLoc(
        "Defend",
        "# Gain !Block! Block.");

    // Reuse the existing large portrait asset until a dedicated small portrait is added.
    public override string PortraitPath => "defend.png".BigCardImagePath();
    public override string BetaPortraitPath => "defend.png".BigCardImagePath();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) => CommonActions.CardBlock(this, play);

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
