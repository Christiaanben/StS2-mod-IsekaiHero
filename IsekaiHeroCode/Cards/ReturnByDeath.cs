using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class ReturnByDeath() : IsekaiHeroCard(2, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override List<(string, string)> Localization => new CardLoc(
        "Return by Death",
        "At the start of your next turn, return to your HP, Block, and status values from when this was played. Lose all Energy next turn. Exhaust.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (Owner?.Creature == null)
            return;

        var savedPowers = Owner.Creature.Powers
            .Select(power => ReturnByDeathPower.Snapshot(power))
            .ToArray();

        var appliedPower = await PowerCmd.Apply<ReturnByDeathPower>(
            Owner.Creature,
            1m,
            Owner.Creature,
            this,
            false);

        if (appliedPower == null)
            return;

        ReturnByDeathPower.SaveState(
            appliedPower,
            Owner.Creature.CurrentHp,
            Owner.Creature.Block,
            savedPowers,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}
