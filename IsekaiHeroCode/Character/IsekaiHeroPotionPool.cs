using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using Godot;

namespace IsekaiHero.IsekaiHeroCode.Character;

public class IsekaiHeroPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => IsekaiHero.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}