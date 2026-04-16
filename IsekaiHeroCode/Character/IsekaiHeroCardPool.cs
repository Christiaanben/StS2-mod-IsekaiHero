using BaseLib.Abstracts;
using IsekaiHero.IsekaiHeroCode.Extensions;
using Godot;

namespace IsekaiHero.IsekaiHeroCode.Character;

public class IsekaiHeroCardPool : CustomCardPoolModel
{
    public override string Title => IsekaiHero.CharacterId; //This is not a display name.
    
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 0.77f; //Hue; deep violet.
    public override float S => 0.72f; //Saturation
    public override float V => 1f; //Brightness
    
    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load IsekaiHero/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => IsekaiHero.Color;
    
    public override bool IsColorless => false;
}
