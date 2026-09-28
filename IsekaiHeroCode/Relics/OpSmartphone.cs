using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Relics;

public sealed class OpSmartphone : IsekaiHeroRelic, ILevelUpRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override List<(string, string)> Localization => new RelicLoc(
        "OP Smartphone",
        "Whenever you Level Up, deal 5 damage to a random enemy.",
        "No signal. Unlimited power.");

    public async Task AfterLevelUp(PlayerChoiceContext choiceContext)
    {
        if (!CombatManager.Instance.IsInProgress || Owner.Creature.IsDead)
            return;

        var enemies = Owner.Creature.CombatState?.HittableEnemies.ToArray();
        if (enemies == null || enemies.Length == 0)
            return;

        var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (target == null)
            return;

        Flash();
        await CreatureCmd.Damage(choiceContext, [target], 5m, ValueProp.Unpowered, Owner.Creature);
    }
}
