using BaseLib.Abstracts;
using BaseLib.Utils;
using IsekaiHero.IsekaiHeroCode.Character;

namespace IsekaiHero.IsekaiHeroCode.Potions;

[Pool(typeof(IsekaiHeroPotionPool))]
public abstract class IsekaiHeroPotion : CustomPotionModel;