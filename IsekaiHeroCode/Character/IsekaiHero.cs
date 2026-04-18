using BaseLib.Abstracts;
using Godot;
using IsekaiHero.IsekaiHeroCode.Cards;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace IsekaiHero.IsekaiHeroCode.Character;

public class IsekaiHero : PlaceholderCharacterModel
{
    public const string CharacterId = "IsekaiHero";

    public static readonly Color Color = new("6C3082");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<VeilOfTheUnseen>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<IsekaiHeroCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<IsekaiHeroRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<IsekaiHeroPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
