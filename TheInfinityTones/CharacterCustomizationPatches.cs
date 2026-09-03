using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using StardewValley.Menus;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu;
using TheInfinityTones.Menus.ColourPickerMenu.Components;

namespace TheInfinityTones;

[HarmonyPatch(typeof(CharacterCustomization))]
public static class CharacterCustomizationPatches
{
    private static ColourWheel? _colourWheel;
    
    [HarmonyPatch(nameof(CharacterCustomization.ResetComponents)), HarmonyPostfix]
    private static void ResetComponents_Postfix(CharacterCustomization __instance)
    {
        if (__instance.source
            is not CharacterCustomization.Source.NewGame
            and not CharacterCustomization.Source.NewFarmhand
            and not CharacterCustomization.Source.HostNewFarm
            and not CharacterCustomization.Source.Wizard
           )
        {
            _colourWheel = null;
            return;
        }
        
        Rectangle randomButtonBounds = __instance.randomButton.bounds;
        _colourWheel = new ColourWheel(
            name: "ColourWheel",
            centerPoint: new Vector2(randomButtonBounds.Center.X, randomButtonBounds.Center.Y + randomButtonBounds.Height + 8),
            width: randomButtonBounds.Width,
            height: randomButtonBounds.Width
        );
        
        if (Game1.options.snappyMenus && Game1.options.gamepadControls && __instance.allClickableComponents is not null)
        {
            __instance.allClickableComponents.Add(_colourWheel);
        }
    }

    [HarmonyPatch(nameof(CharacterCustomization.draw)), HarmonyPostfix]
    private static void draw_Postfix(CharacterCustomization __instance, SpriteBatch b)
    {
        _colourWheel?.draw(b, includeOutline: true);
    }
    
    [HarmonyPatch(nameof(CharacterCustomization.receiveLeftClick)), HarmonyPostfix]
    private static void receiveLeftClick_Postfix(CharacterCustomization __instance, int x, int y)
    {
        if (_colourWheel?.containsPoint(x, y) == true)
        {
            Game1.playSound("drumkit6");
            ModEntry.StoreCustomizationMenu();
            var backupLightest = ModEntry.StoredLightest;
            var backupMedium = ModEntry.StoredMedium;
            var backupDarkest = ModEntry.StoredDarkest;

            var colourPicker = new ColourPickerMenu(onConfirm: (colours) =>
            {
                ModEntry.StoredLightest = colours[0].ToXnaColor();
                ModEntry.StoredMedium = colours[1].ToXnaColor();
                ModEntry.StoredDarkest = colours[2].ToXnaColor();
                ModEntry.GetStoredCustomizationMenu()?._displayFarmer.FarmerRenderer.MarkSpriteDirty();
                ModEntry.RestoreCustomizationMenu();
            }, onCancel: (_) =>
            {
                ModEntry.StoredLightest = backupLightest;
                ModEntry.StoredDarkest = backupDarkest;
                ModEntry.StoredMedium = backupMedium;
                ModEntry.GetStoredCustomizationMenu()?._displayFarmer.FarmerRenderer.MarkSpriteDirty();
                ModEntry.RestoreCustomizationMenu();
            }, drawPreview: ModEntry.PreviewFarmer);
                    
            colourPicker.SetColour(RgbColour.FromXnaColor(ModEntry.StoredLightest ?? ModEntry.DefaultSkinTone[0]), 0);
            colourPicker.SetColour(RgbColour.FromXnaColor(ModEntry.StoredMedium ?? ModEntry.DefaultSkinTone[1]), 1);
            colourPicker.SetColour(RgbColour.FromXnaColor(ModEntry.StoredDarkest ?? ModEntry.DefaultSkinTone[2]), 2);
            colourPicker.ShowPreview();
            colourPicker.ShowAdvancedControls();
            TitleMenu.subMenu = colourPicker;
        }
    }

    [HarmonyPatch(nameof(CharacterCustomization.selectionClick)), HarmonyPostfix]
    private static void selectionClick_Postfix(CharacterCustomization __instance, string name, int change)
    {
        if (name is not "Skin") return;
        ModEntry.StoredLightest = null;
        ModEntry.StoredMedium = null;
        ModEntry.StoredDarkest = null;
    }

    [HarmonyPatch(nameof(CharacterCustomization.performHoverAction)), HarmonyPostfix]
    private static void performHoverAction_Postfix(CharacterCustomization __instance, int x, int y)
    {
        if (_colourWheel?.containsPoint(x, y) == true)
        {
            _colourWheel.Width = (int)MathHelper.Lerp(_colourWheel.Width, __instance.randomButton.bounds.Width * 1.3f, 0.1f);
            _colourWheel.Height = _colourWheel.Width;
        }
        else
        {
            _colourWheel?.Width = (int)MathHelper.Lerp(_colourWheel.Width, __instance.randomButton.bounds.Width, 0.1f);
            _colourWheel?.Height = _colourWheel.Width;
        }

        if (x is 4 && ModEntry.StoredLightest is not null)
        {
            Log.Warn("thing");
        }
    }
    
    [HarmonyPatch(nameof(CharacterCustomization.draw)), HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> draw_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase original)
    {
        var code = instructions.ToList();
        try
        {
            var matcher = new CodeMatcher(code, il);

            matcher.MatchEndForward(
                new CodeMatch(i => i.Calls(AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player)))),
                new CodeMatch(i => i.LoadsField(AccessTools.Field(typeof(Farmer), nameof(Farmer.skin)))),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldc_I4_1)
            ).ThrowIfNotMatch("Failed to find entry point #1.");
            
            matcher.MatchEndForward(
                new CodeMatch(i => i.Calls(AccessTools.Method(typeof(int), nameof(int.ToString))))
            ).ThrowIfNotMatch("Failed to find entry point #2.");
            
            matcher.MatchEndForward(
                new CodeMatch(i => i.IsStloc())
            ).ThrowIfNotMatch("Failed to find entry point #3.");

            LocalBuilder stLoc = matcher.Operand as LocalBuilder ?? throw new Exception("Failed to get sub store.");
            
            matcher.Advance(1);
            matcher.CreateLabel(out Label afterSubBranch);

            matcher.Insert(
	            new CodeInstruction(OpCodes.Ldsflda, AccessTools.Field(typeof(ModEntry), nameof(ModEntry.StoredLightest))),
	            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Color?), nameof(Nullable<>.HasValue))),
	            new CodeInstruction(OpCodes.Brfalse, afterSubBranch),
	            new CodeInstruction(OpCodes.Ldstr, "Custom"),
	            new CodeInstruction(OpCodes.Stloc_S, stLoc)
            );

            return matcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            Log.Error($"Failed to transpile {original.DeclaringType?.FullName}::{original.Name}(): {e}");
            return code;
        }
    }
}