using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace IsekaiHero.IsekaiHeroCode.Relics;

public class TheSystem : IsekaiHeroRelic
{
    private const int StartingExp = 2;
    private const int KillExp = 3;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override List<(string, string)> Localization => new RelicLoc(
        "The System",
        "Whenever an enemy dies, gain 3 EXP. At the start of combat, gain 2 EXP.",
        "Only you can see the interface. Nobody said you couldn't abuse it.");

    public override bool ShouldReceiveCombatHooks => true;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner.Creature || player.PlayerCombatState?.TurnNumber > 1)
            return;

        Flash();
        await LevelPower.GainExp(choiceContext, Owner.Creature, StartingExp, null);
    }

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature target,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (target.Side == Owner.Creature.Side)
            return;

        // Minions (Fatal-ineligible enemies) grant no EXP, matching the Fatal keyword's rules.
        if (!target.Powers.All(power => power.ShouldOwnerDeathTriggerFatal()))
            return;

        Flash();
        await LevelPower.GainExp(choiceContext, Owner.Creature, KillExp, null);
    }
}
