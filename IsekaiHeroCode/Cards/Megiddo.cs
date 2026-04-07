using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public sealed class Megiddo() : IsekaiHeroCard(1, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override List<(string, string)> Localization => new CardLoc(
        "Megiddo",
        "Deal 12 damage to all enemies. Apply 1 Vulnerable. Exhaust.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState?.Enemies == null)
            return;

        await CreatureCmd.Damage(choiceContext, CombatState.Enemies.AsEnumerable(), 12m, default(ValueProp), Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(CombatState.Enemies, 1m, Owner.Creature, this, false);
    }

    protected override void OnUpgrade()
    {
    }
}
