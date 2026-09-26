using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class GameKnowledge() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ExploitPower>(1m)];

    public override List<(string, string)> Localization => new CardLoc(
        "Game Knowledge",
        "# Gain !ExploitPower! Exploit. Draw 1 card.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<ExploitPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ExploitPower"].BaseValue,
            Owner.Creature,
            this,
            false);

        await CardPileCmd.Draw(choiceContext, 1m, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ExploitPower"].UpgradeValueBy(1m);
    }
}
