using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu;
using TheInfinityTones.Menus.ColourPickerMenu.Components;

namespace TheInfinityTones.Patches;

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
            centerPoint: new Vector2(randomButtonBounds.Center.X,
                randomButtonBounds.Center.Y + randomButtonBounds.Height + 8),
            width: randomButtonBounds.Width,
            height: randomButtonBounds.Width
        )
        {
            myID = 6769,
            upNeighborID = 507,
            downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
            leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
            rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
            fullyImmutable = true,
            bounds = new Rectangle(
                randomButtonBounds.X,
                randomButtonBounds.Y + randomButtonBounds.Height + 8,
                randomButtonBounds.Width,
                randomButtonBounds.Width
            )
        };
        
        __instance.randomButton.downNeighborID = _colourWheel.myID;
        
        __instance.allClickableComponents ??= [];
        if (Game1.options.snappyMenus && Game1.options.gamepadControls && __instance.allClickableComponents is not null)
        {
            __instance.allClickableComponents.Add(_colourWheel);
        }
    }

    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.populateClickableComponentList)), HarmonyPostfix]
    private static void populateClickableComponentList_Postfix(IClickableMenu __instance)
    {
        if (__instance is not CharacterCustomization customizationMenu) return;
        if (_colourWheel is null) return;
        customizationMenu.allClickableComponents ??= [];
        if (!customizationMenu.allClickableComponents.Contains(_colourWheel))
        {
            customizationMenu.allClickableComponents.Add(_colourWheel);
        }
    }
    
    [HarmonyPatch(nameof(CharacterCustomization.update)), HarmonyPostfix]
    private static void update_Postfix(CharacterCustomization __instance, GameTime time)
    {
        if (_colourWheel is null) return;
    }

    [HarmonyPatch(nameof(CharacterCustomization.draw)), HarmonyPostfix]
    private static void draw_Postfix(CharacterCustomization __instance, SpriteBatch b)
    {
        _colourWheel?.draw(b, includeOutline: true);
        __instance.drawMouse(b);
    }

    [HarmonyPatch(nameof(CharacterCustomization.optionButtonClick)), HarmonyPostfix]
    private static void optionButtonClick_Postfix(CharacterCustomization __instance, string name)
    {
        if (name is not "OK" || !__instance.canLeaveMenu()) return;
        if (ModEntry.StoredSkinTone.Value is null || Game1.player is null) return;
            
        Game1.player.modData[ModEntry.MOD_DATA_KEY] = ModEntry.StoredSkinTone.Value.ToString();
        Game1.player.FarmerRenderer.MarkSpriteDirty();
        
        ModEntry.BroadcastSkinChange(ModEntry.StoredSkinTone.Value);
            
        ModEntry.StoredSkinTone.Value = null;
        ModEntry.StoredPaletteToggle.Value = null;
        ModEntry.StoredDarkSkinToggle.Value = null;
    }
    
    [HarmonyPatch(nameof(CharacterCustomization.receiveLeftClick)), HarmonyPostfix]
    private static void receiveLeftClick_Postfix(CharacterCustomization __instance, int x, int y)
    {
        if (_colourWheel?.containsPoint(x, y) == true)
        {
            Game1.playSound("drumkit6");
            ModEntry.StoreCustomizationMenu();
            
            if (Game1.player.modData.TryGetValue(ModEntry.MOD_DATA_KEY, out string? skinToneString))
            {
                ModEntry.StoredSkinTone.Value = SkinTone.FromString(skinToneString);
            }
            
            SkinTone backupSkinTone = ModEntry.StoredSkinTone.Value ?? SkinTone.VanillaSkinTones[Game1.player.skin.Value];

            var colourPicker = new ColourPickerMenu(onConfirm: (skinTone) =>
            {
                ModEntry.StoredSkinTone.Value = skinTone;
                
                for (int i = 0; i < SkinTone.VanillaSkinTones.Count; i++)
                {
                    if (ModEntry.StoredSkinTone.Value != SkinTone.VanillaSkinTones[i]) continue;
                    
                    ModEntry.StoredSkinTone.Value = null;
                    Game1.player.skin.Value = i;
                    break;
                }
                
                ModEntry.GetStoredCustomizationMenu()?._displayFarmer.FarmerRenderer.MarkSpriteDirty();
                ModEntry.RestoreCustomizationMenu();
            }, onCancel: (_) =>
            {
                ModEntry.StoredSkinTone.Value = backupSkinTone;
                for (int i = 0; i < SkinTone.VanillaSkinTones.Count; i++)
                {
                    if (backupSkinTone != SkinTone.VanillaSkinTones[i]) continue;
                    
                    ModEntry.StoredSkinTone.Value = null;
                    Game1.player.skin.Value = i;
                    break;
                }
                
                ModEntry.GetStoredCustomizationMenu()?._displayFarmer.FarmerRenderer.MarkSpriteDirty();
                ModEntry.RestoreCustomizationMenu();
            }, drawPreview: ModEntry.PreviewFarmer);
                    
            colourPicker.SetColour(RgbColour.FromXnaColor(backupSkinTone.Lightest), 0);
            colourPicker.SetColour(RgbColour.FromXnaColor(backupSkinTone.Medium), 1);
            colourPicker.SetColour(RgbColour.FromXnaColor(backupSkinTone.Darkest), 2);
            colourPicker.ShowPreview();
            colourPicker.ShowAdvancedControls();
            colourPicker.update(Game1.currentGameTime);
            if (Game1.options.SnappyMenus) colourPicker.snapToDefaultClickableComponent();
            
            if (Game1.activeClickableMenu is TitleMenu) TitleMenu.subMenu = colourPicker;
            else Game1.activeClickableMenu = colourPicker;
        }
    }

    [HarmonyPatch(nameof(CharacterCustomization.selectionClick)), HarmonyPostfix]
    private static void selectionClick_Postfix(CharacterCustomization __instance, string name, int change)
    {
        if (name is not "Skin") return;
        ModEntry.StoredSkinTone.Value = null;
        ModEntry.StoredPaletteToggle.Value = null;
        ModEntry.StoredDarkSkinToggle.Value = null;
        Game1.player.modData.Remove(ModEntry.MOD_DATA_KEY);
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

            LocalBuilder perScreenLocal = il.DeclareLocal(typeof(SkinTone?));
            Label customLabel = il.DefineLabel();
            matcher.Insert(
	            new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ModEntry), nameof(ModEntry.StoredSkinTone))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PerScreen<SkinTone?>), nameof(PerScreen<>.Value))),
                new CodeInstruction(OpCodes.Stloc, perScreenLocal),
                new CodeInstruction(OpCodes.Ldloca, perScreenLocal),
	            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(SkinTone?), nameof(Nullable<>.HasValue))),
	            new CodeInstruction(OpCodes.Brtrue, customLabel),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.modData))),
                new CodeInstruction(OpCodes.Ldstr, ModEntry.MOD_DATA_KEY),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ModDataDictionary), nameof(ModDataDictionary.ContainsKey))),
                new CodeInstruction(OpCodes.Brfalse, afterSubBranch),
	            new CodeInstruction(OpCodes.Ldstr, "Custom").WithLabels(customLabel),
	            new CodeInstruction(OpCodes.Stloc, stLoc)
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