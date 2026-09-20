using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using SpiderCore.Common.Extensions;
using SpiderCore.Common.Logging;
using SpiderCore.Common.Menus.ColourPicker;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewModdingAPI;
// ReSharper disable UnusedMember.Local

namespace UnlimitedChestColours.Patches;

[HarmonyPatch(typeof(ItemGrabMenu))]
public static class ItemGrabMenuPatches
{
    private static ColourWheel? _colourWheel;
    
    [HarmonyPostfix, HarmonyPatch(MethodType.Constructor, typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object), typeof(ItemExitBehavior), typeof(bool))]
    private static void Constructor_Postfix(ItemGrabMenu __instance)
    {
        if (!__instance.CanHaveColorPicker() || __instance.sourceItem is not Chest) return;
        
        Rectangle wheelBounds = new Rectangle(
            __instance.colorPickerToggleButton.bounds.X,
            __instance.colorPickerToggleButton.bounds.Y,
            __instance.colorPickerToggleButton.bounds.Width,
            __instance.colorPickerToggleButton.bounds.Width
        );
        _colourWheel = new ColourWheel(
            name: "ColourWheel",
            centerPoint: new Vector2(__instance.colorPickerToggleButton.bounds.Center.X, __instance.colorPickerToggleButton.bounds.Center.Y),
            width: 40,
            height: 40
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
        
        __instance.chestColorPicker = null;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ItemGrabMenu.setSourceItem))]
    private static void setSourceItem_Prefix(ItemGrabMenu __instance, ref Color __state)
    {
        if (!__instance.CanHaveColorPicker() || __instance.sourceItem is not Chest chest) return;
        
        __state = chest.playerChoiceColor.Value;
        __instance.chestColorPicker = null;
    }
    
    [HarmonyPostfix, HarmonyPatch(nameof(ItemGrabMenu.setSourceItem))]
    private static void setSourceItem_Postfix(ItemGrabMenu __instance, Color __state)
    {
        if (!__instance.CanHaveColorPicker() || __instance.sourceItem is not Chest chest) return;
        
        chest.playerChoiceColor.Value = __state;
        __instance.chestColorPicker = null;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ItemGrabMenu.gameWindowSizeChanged))]
    private static void gameWindowSizeChanged_Postfix(ItemGrabMenu __instance)
    {
        if (!__instance.CanHaveColorPicker() || __instance.sourceItem is not Chest) return;

        __instance.chestColorPicker = null;
    }
    
    [HarmonyPostfix, HarmonyPatch(nameof(ItemGrabMenu.draw), typeof(SpriteBatch))]
    private static void draw_Postfix(ItemGrabMenu __instance, SpriteBatch b)
    {
        if (!__instance.CanHaveColorPicker() || __instance.colorPickerToggleButton is null || _colourWheel is null) return;

        Rectangle pixelSourceRect = new Rectangle(
            x: 105,
            y: 482,
            width: 1,
            height: 1
        );
        b.Draw(
            texture: Game1.mouseCursors,
            destinationRectangle: new Rectangle(
                x: __instance.colorPickerToggleButton.bounds.X + 8,
                y: __instance.colorPickerToggleButton.bounds.Y + 8,
                width: 48, 
                height: 48),
            sourceRectangle: pixelSourceRect,
            color: Color.White
        );
        
        _colourWheel.CenterPoint = __instance.colorPickerToggleButton.bounds.Center.ToVector2();
        _colourWheel.draw(b, includeOutline: true);

        if (ModEntry.ModHelper.Input.IsAnyDown(SButton.LeftShift, SButton.LeftTrigger))
        {
            b.Draw(
                texture: Game1.mouseCursors,
                destinationRectangle: new Rectangle(
                    x: (int)_colourWheel.CenterPoint.X,
                    y: (int)_colourWheel.CenterPoint.Y,
                    width: (int)_colourWheel.Width, 
                    height: (int)_colourWheel.Width
                ),
                sourceRectangle: new Rectangle(
                    x: 269,
                    y: 471,
                    width: 14,
                    height: 14
                ),
                color: Color.White,
                rotation: 0f,
                origin: new Vector2(7f, 7f),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }

        __instance.drawMouse(b);
    }

    [HarmonyPrefix, HarmonyPatch(nameof(ItemGrabMenu.receiveLeftClick))]
    private static bool receiveLeftClick_Prefix(ItemGrabMenu __instance, int x, int y)
    {
        if (!__instance.CanHaveColorPicker() || _colourWheel is null || __instance.sourceItem is not Chest chest) return true;
        if (__instance.colorPickerToggleButton is null || !__instance.colorPickerToggleButton.containsPoint(x, y)) return true;

        _colourWheel.Width = 40;
        if (ModEntry.ModHelper.Input.IsAnyDown(SButton.LeftShift, SButton.LeftTrigger))
        {
            chest.playerChoiceColor.Value = Color.Black;
            Game1.playSound("drumkit6");
            return false;
        }
            
        ModEntry.OpenColourPicker(__instance);
        Game1.playSound("bigSelect");
        return false;

    }
    
    [HarmonyPostfix, HarmonyPatch(nameof(ItemGrabMenu.performHoverAction))]
    private static void performHoverAction_Postfix(ItemGrabMenu __instance, int x, int y)
    {
        if (!__instance.CanHaveColorPicker() || _colourWheel is null) return;
        
        if (__instance.colorPickerToggleButton?.containsPoint(x, y) == true)
        {
            _colourWheel.Width = (int)MathHelper.Lerp(_colourWheel.Width, 40 * 1.35f, 0.1f);
            _colourWheel.Height = _colourWheel.Width;
        }
        else
        {
            _colourWheel.Width = (int)MathHelper.Lerp(_colourWheel.Width, 40, 0.1f);
            _colourWheel.Height = _colourWheel.Width;
        }
        
        if (ModEntry.ModHelper.Input.IsAnyDown(SButton.LeftShift, SButton.LeftTrigger) && __instance.hoverText == __instance.colorPickerToggleButton?.hoverText)
        {
            __instance.hoverText = ModEntry.ModHelper.Translation.Get("ResetChestColour");
        }
    }
}