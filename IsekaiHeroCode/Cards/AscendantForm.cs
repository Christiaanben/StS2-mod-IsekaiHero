using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class AscendantForm() : IsekaiHeroCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bypasses", 2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ExploitPower>()];

    public override List<(string, string)> Localization => new CardLoc(
        "Ascendant Form",
        "# Your first !Bypasses! unmet Exploit conditions each turn count as met for free.\nDoes not stack.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var current = Owner.Creature.Powers.OfType<AscendantFormPower>().FirstOrDefault()?.Amount ?? 0;
        if (DynamicVars["Bypasses"].IntValue > current)
            await PowerCmd.Apply<AscendantFormPower>(choiceContext, Owner.Creature,
                DynamicVars["Bypasses"].IntValue - current, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Bypasses"].UpgradeValueBy(1m);
}
