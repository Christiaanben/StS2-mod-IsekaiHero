using BaseLib.Abstracts;
using Godot;
using IsekaiHero.IsekaiHeroCode.Cards;
using IsekaiHero.IsekaiHeroCode.Extensions;
using IsekaiHero.IsekaiHeroCode.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace IsekaiHero.IsekaiHeroCode.Character;

public class IsekaiHero : PlaceholderCharacterModel
{
    public const string CharacterId = "IsekaiHero";

    public static readonly Color Color = new("6C3082");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override NCreatureVisuals CreateCustomVisuals() => IsekaiHeroVisuals.Create();
    public override float DeathAnimTime => 1.2f;

    // The quick PCK packer cannot ship .tscn files. Use BaseLib's simple counter
    // with one painted layer and transparent placeholders for the other layers.
    public override string CustomEnergyCounterPath => null!;
    public override CustomEnergyCounter? CustomEnergyCounter => new(
        layer => layer == 1 ? EnergyDiamondPath : EmptyEnergyLayerPath,
        new Color("182D50"), new Color("55DDE8"));
    public override Color EnergyLabelOutlineColor => new("182D50");

    private const string EnergyDiamondPath = "res://IsekaiHero/images/charui/energy_diamond_v1.png";
    private const string EmptyEnergyLayerPath = "res://IsekaiHero/images/charui/energy_empty_layer.png";

    protected override IEnumerable<string> ExtraAssetPaths => base.ExtraAssetPaths.Concat(
        [IsekaiHeroVisuals.AtlasPath, IsekaiHeroSelectArt.TexturePath, EnergyDiamondPath, EmptyEnergyLayerPath]);

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeIsekaiHero>(),
        ModelDb.Card<StrikeIsekaiHero>(),
        ModelDb.Card<StrikeIsekaiHero>(),
        ModelDb.Card<StrikeIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<DefendIsekaiHero>(),
        ModelDb.Card<Grind>(),
        ModelDb.Card<DangerSense>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<TheSystem>()
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
