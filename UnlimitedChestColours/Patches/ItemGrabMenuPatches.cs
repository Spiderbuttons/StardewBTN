using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiderCore.Common.Logging;
using SpiderCore.Common.Menus.ColourPicker.Components;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace UnlimitedChestColours.Patches;

[HarmonyPatch(typeof(ItemGrabMenu))]
public static class ItemGrabMenuPatches
{
    private static ColourWheel? _colourWheel;
    
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor, typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object), typeof(ItemExitBehavior), typeof(bool))]
    public static void Constructor_Postfix(ItemGrabMenu __instance)
    {
        if (!__instance.CanHaveColorPicker() || __instance.sourceItem is not Chest chest) return;

        Log.Debug(__instance.colorPickerToggleButton.bounds);
        
        Rectangle wheelBounds = new Rectangle(
            __instance.colorPickerToggleButton.bounds.X,
            __instance.colorPickerToggleButton.bounds.Y,
            __instance.colorPickerToggleButton.bounds.Width,
            __instance.colorPickerToggleButton.bounds.Width
        );
        _colourWheel = new ColourWheel(
            name: "ColourWheel",
            centerPoint: new Vector2(__instance.colorPickerToggleButton.bounds.Center.X,
                __instance.colorPickerToggleButton.bounds.Center.Y),
            width: __instance.colorPickerToggleButton.bounds.Width - 6,
            height: __instance.colorPickerToggleButton.bounds.Width - 6
        )
        {
            myID = 27346,
            downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
            leftNeighborID = 53921,
            region = 15923,
            bounds = wheelBounds
        };
        
        __instance.allClickableComponents ??= [];
        if (Game1.options.snappyMenus && Game1.options.gamepadControls && __instance.allClickableComponents is not null)
        {
            __instance.allClickableComponents.Remove(__instance.colorPickerToggleButton);
            __instance.allClickableComponents.Add(_colourWheel);
        }

        __instance.colorPickerToggleButton.visible = false;
    }
    
    [HarmonyPostfix, HarmonyPatch(nameof(ItemGrabMenu.draw), typeof(SpriteBatch))]
    public static void Draw_Postfix(ItemGrabMenu __instance, SpriteBatch b)
    {
        if (!__instance.CanHaveColorPicker() || _colourWheel is null) return;
        Rectangle wheelBounds = new Rectangle(
            __instance.colorPickerToggleButton.bounds.X,
            __instance.colorPickerToggleButton.bounds.Y,
            __instance.colorPickerToggleButton.bounds.Width,
            __instance.colorPickerToggleButton.bounds.Width
        );
        _colourWheel.CenterPoint = wheelBounds.Center.ToVector2();
        _colourWheel.draw(b, includeOutline: true);
    }
}