using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
namespace IsekaiHero.IsekaiHeroCode.Relics;

public class HealingSigil : IsekaiHeroRelic
{
    public override MegaCrit.Sts2.Core.Entities.Relics.RelicRarity Rarity =>
        MegaCrit.Sts2.Core.Entities.Relics.RelicRarity.Starter;

    public override List<(string, string)> Localization => new RelicLoc(
        "Healing Sigil",
        "At the start of combat, heal 3 HP.",
        "A quiet charm that steadies the hero before the fight begins.");

    public override bool ShouldReceiveCombatHooks => true;

    public override async Task BeforeCombatStart()
    {
        await CreatureCmd.Heal(Owner.Creature, 3);
    }
}
