using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace IsekaiHero.IsekaiHeroCode.Cards;

public static class IsekaiHeroKeywords
{
    [CustomEnum("OVERRIDE")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Override = CardKeyword.None;
}
