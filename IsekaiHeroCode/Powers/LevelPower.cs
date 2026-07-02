using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IsekaiHero.IsekaiHeroCode.Powers;

/// <summary>
/// The EXP/Level engine. Amount = current Level. EXP progress toward the next
/// level is tracked as internal data. Class-agnostic by design: any creature
/// that gains EXP levels up and gets the Vigor ding, so IsekaiHero cards stay
/// playable in cross-pool contexts.
/// </summary>
public sealed class LevelPower : IsekaiHeroPower
{
    public const int ExpPerLevel = 4;
    public const int MaxLevel = 10;
    public const int VigorPerLevelUp = 2;

    private sealed class Data
    {
        public int Exp;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Level",
        "You are Level {Amount} (max 10). Every 4 EXP is a Level Up. When you Level Up, gain 2 Vigor.",
        "You are Level {Amount} (max 10). Every 4 EXP is a Level Up. When you Level Up, gain 2 Vigor.");

    protected override object InitInternalData()
    {
        return new Data();
    }

    public static int GetLevel(Creature? creature)
    {
        return creature?.Powers.OfType<LevelPower>().FirstOrDefault()?.Amount ?? 0;
    }

    public static async Task GainExp(
        PlayerChoiceContext choiceContext,
        Creature creature,
        int amount,
        CardModel? source)
    {
        if (amount <= 0)
            return;

        var power = creature.Powers.OfType<LevelPower>().FirstOrDefault();
        if (power == null)
        {
            await PowerCmd.Apply<LevelPower>(choiceContext, creature, 1, creature, source, false);
            power = creature.Powers.OfType<LevelPower>().FirstOrDefault();
            if (power == null)
                return;
        }

        var data = power.GetInternalData<Data>();
        data.Exp += amount;
        power.Flash();

        // At MaxLevel, surplus EXP stays banked (a future "Break the Level Cap"
        // effect can spend it) instead of being consumed for nothing.
        while (data.Exp >= ExpPerLevel && power.Amount < MaxLevel)
        {
            data.Exp -= ExpPerLevel;
            await PowerCmd.Apply<LevelPower>(choiceContext, creature, 1, creature, source, false);
            await PowerCmd.Apply<VigorPower>(choiceContext, creature, VigorPerLevelUp, creature, source, false);
        }
    }
}
