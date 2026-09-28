using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace IsekaiHero.IsekaiHeroCode.Relics;

// Dispatched by the EXP engine once per actual increase, after its Vigor reward.
public interface ILevelUpRelic
{
    Task AfterLevelUp(PlayerChoiceContext choiceContext);
}
