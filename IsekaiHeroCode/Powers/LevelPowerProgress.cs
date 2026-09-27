using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace IsekaiHero.IsekaiHeroCode.Powers;

// Use the same refresh path as the level label, including initial creation and
// model replacement. No polling or extra model-event subscriptions are needed.
[HarmonyPatch(typeof(NPower), "RefreshAmount")]
internal static class LevelPowerProgress
{
    private const string BarName = "IsekaiLevelExperience";

    [HarmonyPostfix]
    private static void Postfix(NPower __instance, PowerModel? ____model)
    {
        var bar = __instance.GetNodeOrNull<Control>(BarName);
        if (____model is not LevelPower level)
        {
            if (bar != null) bar.Visible = false;
            return;
        }

        if (bar == null)
        {
            bar = new Control
            {
                Name = BarName,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AnchorTop = 1,
                AnchorBottom = 1,
                AnchorRight = 1,
                OffsetTop = 2,
                OffsetBottom = 8
            };
            __instance.AddChild(bar);
            for (var i = 0; i < LevelPower.ExpPerLevel; i++)
            {
                var slot = new ColorRect
                {
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Color = new Color("101c25"),
                    AnchorLeft = i / (float)LevelPower.ExpPerLevel,
                    AnchorRight = (i + 1) / (float)LevelPower.ExpPerLevel,
                    AnchorBottom = 1,
                    OffsetLeft = 1,
                    OffsetRight = -1
                };
                bar.AddChild(slot);
                var fill = new ColorRect
                {
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    AnchorRight = 1,
                    AnchorBottom = 1,
                    OffsetLeft = 1,
                    OffsetRight = -1,
                    OffsetTop = 1,
                    OffsetBottom = -1
                };
                slot.AddChild(fill);
            }
        }

        bar.Visible = true;
        for (var i = 0; i < LevelPower.ExpPerLevel; i++)
        {
            var fill = bar.GetChild(i).GetChild<ColorRect>(0);
            fill.Color = level.IsMaxLevel ? new Color("ffd477") : new Color("65eadb");
            fill.Visible = level.IsMaxLevel || i < level.Experience;
        }
    }
}
