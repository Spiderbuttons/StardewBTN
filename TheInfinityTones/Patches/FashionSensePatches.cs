using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using TheInfinityTones.Helpers;

namespace TheInfinityTones.Patches;

public static class FashionSensePatches
{
    private static ConstructorInfo SkinToneModelCtor = null!;
    
    public static void Patch(Harmony harmony)
    {
        if (!ModEntry.ModHelper.ModRegistry.IsLoaded("PeacefulEnd.FashionSense")) return;
        Log.Trace("Fashion Sense detected, applying compatibility patch...");
        
        try
        {
            Type skinToneModelType = AccessTools.TypeByName("FashionSense.Framework.Models.Appearances.SkinToneModel") ?? throw new Exception("Could not find SkinToneModel type.");
            SkinToneModelCtor = AccessTools.Constructor(skinToneModelType, [typeof(Color), typeof(Color), typeof(Color)]) ?? throw new Exception("Could not find SkinToneModel constructor.");
            
            harmony.Patch(
                original: AccessTools.Method(typeColonName: "FashionSense.Framework.Patches.Renderer.DrawPatch:GetSkinTone"),
                postfix: new HarmonyMethod(methodType: typeof(FashionSensePatches), methodName: nameof(DrawPatch_GetSkinTone_Postfix))
            );
        } 
        catch (Exception ex)
        {
            Log.Error(message: $"Failed to patch Fashion Sense: {ex}");
        }
    }

    public static void DrawPatch_GetSkinTone_Postfix(object[] __args, ref object __result)
    {
        if (ModEntry.StoredSkinTone.Value is { } storedTone)
        {
            var model = SkinToneModelCtor.Invoke([storedTone.Lightest, storedTone.Medium, storedTone.Darkest]);
            __result = model;
            return;
        }

        if (__args[^1] is not Farmer farmer) return;
        SkinTone skinTone = SkinTone.GetSkinToneFromFarmer(farmer);
        var modelFromFarmer = SkinToneModelCtor.Invoke([skinTone.Lightest, skinTone.Medium, skinTone.Darkest]);
        __result = modelFromFarmer;
    }
}