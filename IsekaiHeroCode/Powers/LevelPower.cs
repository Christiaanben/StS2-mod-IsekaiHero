using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Relics;
using IsekaiHero.IsekaiHeroCode.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace IsekaiHero.IsekaiHeroCode.Powers;

/// <summary>
/// The EXP/Level engine. Amount = current Level. EXP progress toward the next
/// level is tracked as internal data. Class-agnostic by design: any creature
/// that gains EXP levels up and gets combat-long stat rewards, so IsekaiHero cards stay
/// playable in cross-pool contexts.
/// </summary>
public sealed class LevelPower : IsekaiHeroPower
{
    public const int ExpPerLevel = 4;
    public const int MaxLevel = 10;

    private sealed class Data
    {
        public int Exp;
        public bool Processing;
        public int LastLevelUpRound = -1;
        public CombatSide LastLevelUpSide;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public int Experience => IsMutable ? GetInternalData<Data>().Exp : 0;
    public bool IsMaxLevel => Amount >= MaxLevel;

    public override List<(string, string)> Localization => new PowerLoc(
        "Level",
        "You are Level {Amount} (max 10). Every 4 EXP is a Level Up. When you reach an even Level, gain 1 Strength. When you reach an odd Level, gain 1 Dexterity. At Levels 5 and 10, gain {energyPrefix:energyIcons(1)} immediately and at the start of your turn.",
        "You are Level {Amount} (max 10). Every 4 EXP is a Level Up. When you reach an even Level, gain 1 Strength. When you reach an odd Level, gain 1 Dexterity. At Levels 5 and 10, gain {energyPrefix:energyIcons(1)} immediately and at the start of your turn.",
        ("expTitle", "EXP Progress"),
        ("expProgress", "{Exp}/4 EXP toward the next level. Each filled segment represents 1 EXP."),
        ("expMax", "Maximum level reached. {Exp} EXP banked. The gold bar indicates the level cap."));

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var description = new LocString("powers", Id.Entry + (IsMaxLevel ? ".expMax" : ".expProgress"));
            description.Add("Exp", Experience);
            yield return HoverTipFactory.FromPower<StrengthPower>();
            yield return HoverTipFactory.FromPower<DexterityPower>();
            yield return HoverTipFactory.ForEnergy(this);
            yield return new HoverTip(new LocString("powers", Id.Entry + ".expTitle"), description);
        }
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        return player == Owner.Player ? amount + Amount / 5 : amount;
    }

    public static int GetLevel(Creature? creature)
    {
        return creature?.Powers.OfType<LevelPower>().FirstOrDefault()?.Amount ?? 0;
    }

    public static bool LeveledUpThisTurn(Creature creature)
    {
        var power = creature.Powers.OfType<LevelPower>().FirstOrDefault();
        if (power == null || creature.CombatState is not { } combat)
            return false;
        var data = power.GetInternalData<Data>();
        return data.LastLevelUpRound == combat.RoundNumber && data.LastLevelUpSide == combat.CurrentSide;
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

        // A relic kill can grant EXP during a reaction. Bank it for this same
        // loop instead of entering a second Level-Up/reaction chain.
        if (data.Processing)
            return;
        data.Processing = true;

        // At MaxLevel, surplus EXP stays banked (a future "Break the Level Cap"
        // effect can spend it) instead of being consumed for nothing.
        try
        {
            while (data.Exp >= ExpPerLevel && power.Amount < MaxLevel &&
                   CombatManager.Instance.IsInProgress && !creature.IsDead)
            {
                data.Exp -= ExpPerLevel;
                await PowerCmd.Apply<LevelPower>(choiceContext, creature, 1, creature, source, false);
                data.LastLevelUpRound = creature.CombatState?.RoundNumber ?? -1;
                data.LastLevelUpSide = creature.CombatState?.CurrentSide ?? CombatSide.Player;
                if (power.Amount % 2 == 0)
                    await PowerCmd.Apply<StrengthPower>(choiceContext, creature, 1, creature, source, false);
                else
                    await PowerCmd.Apply<DexterityPower>(choiceContext, creature, 1, creature, source, false);

                if (power.Amount % 5 == 0 && creature.Player is { } energyPlayer)
                    await PlayerCmd.GainEnergy(1, energyPlayer);

                // Snapshot the hand before later Level-Up reactions can change it.
                if (creature.Player is { } player)
                {
                    var judgments = CardPile.Get(PileType.Hand, player)?.Cards
                        .OfType<HerosJudgment>().ToArray() ?? [];
                    foreach (var judgment in judgments)
                        judgment.GainLevelDamage();
                }

                var reactions = creature.Player?.Relics.OfType<ILevelUpRelic>().ToArray() ?? [];
                foreach (var reaction in reactions)
                {
                    if (!CombatManager.Instance.IsInProgress || creature.IsDead)
                        break;
                    await reaction.AfterLevelUp(choiceContext);
                }
            }
        }
        finally
        {
            data.Processing = false;
            // EXP-only gains must refresh the UI even when the level did not change.
            power.InvokeDisplayAmountChanged();
        }
    }
}
