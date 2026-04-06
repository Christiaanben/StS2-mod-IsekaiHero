using BaseLib.Abstracts;
using BaseLib.Extensions;
using IsekaiHero.IsekaiHeroCode.Extensions;
using Godot;

namespace IsekaiHero.IsekaiHeroCode.Powers;

public abstract class IsekaiHeroPower : CustomPowerModel
{
    //Loads from IsekaiHero/images/powers/your_power.png
    public override string CustomPackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".PowerImagePath();
        }
    }

    public override string CustomBigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
            return ResourceLoader.Exists(path) ? path : "power.png".BigPowerImagePath();
        }
    }
}