using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace IsekaiHero.IsekaiHeroCode.Relics;

// Dispatched by the EXP engine once per actual increase, after its stat and energy rewards.
public interface ILevelUpRelic
{
    Task AfterLevelUp(PlayerChoiceContext choiceContext);
}
