using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TheInfinityTones.Helpers;

namespace TheInfinityTones.Patches;

// https://www.nexusmods.com/stardewvalley/mods/46160
public static class SaveAndLoadCharacterPresetsPatches
{
    public static void Patch(Harmony harmony)
    {
        if (!ModEntry.ModHelper.ModRegistry.IsLoaded("dylanjames.charactercreation")) return;
        Log.Trace("Save and Load Character Presets detected, applying compatibility patch...");
        
        try
        {
            Type menuType = AccessTools.TypeByName("CharacterCreation.FarmerPresetCustomizationMenu") ?? throw new Exception("Could not find FarmerPresetCustomizationMenu type.");
            harmony.Patch(
                original: AccessTools.Method(menuType, "CreateButton"),
                prefix: new HarmonyMethod(methodType: typeof(SaveAndLoadCharacterPresetsPatches), methodName: nameof(FarmerPresetCustomizationMenu_CreateButton_Prefix))
            );
        } 
        catch (Exception ex)
        {
            Log.Error(message: $"Failed to patch Save and Load Character Presets: {ex}");
        }
    }

    // Lil nudge.
    private static void FarmerPresetCustomizationMenu_CreateButton_Prefix(ref Rectangle bounds)
    {
        bounds.Y += 48;
    }
}