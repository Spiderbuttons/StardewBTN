using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewValley.Extensions;
using TheInfinityTones.Helpers;

namespace TheInfinityTones.Patches;

// https://www.nexusmods.com/stardewvalley/mods/46160
public static class SaveAndLoadCharacterPresetsPatches
{
    private static Type? PresetSlotType;
    private static MethodInfo? PresetSlotSummaryGetter;
    private static MethodInfo? PresetSummaryIdGetter;

    private static IModHelper? SaveAndLoadPresetsHelper => SCore.Instance.ModRegistry.Get("dylanjames.charactercreation")?.Mod?.Helper ?? null;
    
    public static void Patch(Harmony harmony)
    {
        if (!ModEntry.ModHelper.ModRegistry.IsLoaded("dylanjames.charactercreation")) return;
        Log.Trace("Save and Load Character Presets detected, applying compatibility patch...");
        
        try
        {
            harmony.Patch(
                original: AccessTools.Method("CharacterCreation.FarmerPresetCustomizationMenu:CreateButton"),
                prefix: new HarmonyMethod(methodType: typeof(SaveAndLoadCharacterPresetsPatches), methodName: nameof(FarmerPresetCustomizationMenu_CreateButton_Prefix))
            );
            
            PresetSlotType = AccessTools.TypeByName("CharacterCreation.FarmerPresetLoadMenu+PresetSlot") ?? throw new Exception("Failed to find CharacterCreation.FarmerPresetLoadMenu.PresetSlot type.");
            PresetSlotSummaryGetter = AccessTools.PropertyGetter(PresetSlotType, "Summary") ?? throw new Exception("Failed to find CharacterCreation.FarmerPresetLoadMenu.PresetSlot:Summary property.");
            PresetSummaryIdGetter = AccessTools.PropertyGetter(PresetSlotSummaryGetter.ReturnType, "Id") ?? throw new Exception("Failed to find CharacterCreation.PresetSummary:Id property.");

            harmony.Patch(
                original: AccessTools.Method("CharacterCreation.FarmerPresetManager:SavePreset"),
                transpiler: new HarmonyMethod(methodType: typeof(SaveAndLoadCharacterPresetsPatches), methodName: nameof(FarmerPresetManager_SavePreset_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method("CharacterCreation.FarmerPresetLoadMenu:LoadPreset"),
                postfix: new HarmonyMethod(methodType: typeof(SaveAndLoadCharacterPresetsPatches), methodName: nameof(FarmerPresetLoadMenu_LoadPreset_Postfix))
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

    private static void SavePresetSkinData(string relativePath)
    {
        if (SaveAndLoadPresetsHelper is null || !ModEntry.StoredSkinTone.Value.HasValue) return;
        string topFolder = Path.GetDirectoryName(relativePath) ?? string.Empty;
        string fileName = Path.GetFileName(relativePath);
        string skinDataPath = Path.Combine(topFolder, ModEntry.UNIQUE_ID.ToLowerInvariant(), fileName);
        SaveAndLoadPresetsHelper.Data.WriteJsonFile(skinDataPath, ModEntry.StoredSkinTone.Value.ToString());
    }

    private static void FarmerPresetLoadMenu_LoadPreset_Postfix(object[] __args)
    {
        if (SaveAndLoadPresetsHelper is null || __args[0].GetType() != PresetSlotType) return;
        
        var presetSlot = __args[0];
        var summary = PresetSlotSummaryGetter?.Invoke(presetSlot, null);
        var presetId = (string?)PresetSummaryIdGetter?.Invoke(summary, null);
        
        if (string.IsNullOrEmpty(presetId)) return;
        
        var skinDataPath = Path.Combine("presets", ModEntry.UNIQUE_ID.ToLowerInvariant(), presetId + ".json");
        var skinData = SaveAndLoadPresetsHelper.Data.ReadJsonFile<string>(skinDataPath);
        if (string.IsNullOrEmpty(skinData)) return;
        
        SkinTone tone = SkinTone.FromString(skinData);
        ModEntry.StoredSkinTone.Value = tone;
    }

    private static IEnumerable<CodeInstruction> FarmerPresetManager_SavePreset_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase original)
    {
        var code = instructions.ToList();
        try
        {
            var matcher = new CodeMatcher(code, il);

            matcher.MatchEndForward(
                new CodeMatch(i => i.operand is MethodInfo info && info.Name.ContainsIgnoreCase("WriteJsonFile"))
            ).ThrowIfNotMatch("Failed to find WriteJsonFile call. Somehow.");

            matcher.Advance(1);

            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SaveAndLoadCharacterPresetsPatches), nameof(SavePresetSkinData)))
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