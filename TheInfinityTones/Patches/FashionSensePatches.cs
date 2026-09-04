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
    private static MethodInfo? GetBodyColorMethod;
    private static object? TextureManagerInstance;
    private static MethodInfo? GetSpecificAppearanceModelMethod;
    private static MethodInfo? GetBodyFromFacingDirectionMethod;
    
    public static void Patch(Harmony harmony)
    {
        if (!ModEntry.ModHelper.ModRegistry.IsLoaded("PeacefulEnd.FashionSense")) return;
        Log.Trace("Fashion Sense detected, applying compatibility patch...");
        
        try
        {
            Type drawPatchType = AccessTools.TypeByName("FashionSense.Framework.Patches.Renderer.DrawPatch") ?? throw new Exception("Could not find DrawPatch type.");
            
            Type skinToneModelType = AccessTools.TypeByName("FashionSense.Framework.Models.Appearances.SkinToneModel") ?? throw new Exception("Could not find SkinToneModel type.");
            SkinToneModelCtor = AccessTools.Constructor(skinToneModelType, [typeof(Color), typeof(Color), typeof(Color)]) ?? throw new Exception("Could not find SkinToneModel constructor.");
            
            harmony.Patch(
                original: AccessTools.Method(drawPatchType, "GetSkinTone"),
                postfix: new HarmonyMethod(methodType: typeof(FashionSensePatches), methodName: nameof(DrawPatch_GetSkinTone_Postfix))
            );
            
            GetBodyColorMethod = AccessTools.Method(drawPatchType, "GetBodyColor") ?? throw new Exception("Could not find GetBodyColor method.");
            
            Type fashionSenseType = AccessTools.TypeByName("FashionSense.FashionSense") ?? throw new Exception("Could not find FashionSense type.");
            TextureManagerInstance = AccessTools.Field(fashionSenseType, "textureManager")?.GetValue(null) ?? throw new Exception("Could not find TextureManager instance.");

            Type bodyContentPackType = AccessTools.TypeByName("FashionSense.Framework.Models.Appearances.Body.BodyContentPack") ?? throw new Exception("Could not find BodyContentPack type.");
            GetBodyFromFacingDirectionMethod = AccessTools.Method(bodyContentPackType, "GetBodyFromFacingDirection") ??
                                               throw new Exception("Could not find GetBodyFromFacingDirection method.");
            
            Type textureManagerType = AccessTools.TypeByName("FashionSense.Framework.Managers.TextureManager") ?? throw new Exception("Could not find TextureManager type.");
            GetSpecificAppearanceModelMethod = AccessTools.Method(textureManagerType, "GetSpecificAppearanceModel")?.MakeGenericMethod(bodyContentPackType) ??
                                               throw new Exception("Could not find GetSpecificAppearanceModel method.");
        } 
        catch (Exception ex)
        {
            Log.Error(message: $"Failed to patch Fashion Sense: {ex}");
        }
    }

    public static void DrawPatch_GetSkinTone_Postfix(object[] __args, ref object __result)
    {
        if (__args[^1] is not Farmer farmer || // Farmer who 
            __args[^2] is true || // bool sickFrame
            (GetBodyColorMethod?.Invoke(null, [farmer]) is not null && GetBodyModel(farmer) is not null) ||
            (farmer.modData.TryGetValue("FashionSense.CustomBody.Id", out var val) && val == "Override Body Color")) return;
        
        if (ModEntry.StoredSkinTone.Value is { } storedTone)
        {
            var model = SkinToneModelCtor.Invoke([storedTone.Lightest, storedTone.Medium, storedTone.Darkest]);
            __result = model;
            return;
        }
        
        SkinTone skinTone = SkinTone.GetSkinToneFromFarmer(farmer);
        var modelFromFarmer = SkinToneModelCtor.Invoke([skinTone.Lightest, skinTone.Medium, skinTone.Darkest]);
        __result = modelFromFarmer;
    }

    private static object? GetBodyModel(Farmer who)
    {
        if (!who.modData.TryGetValue("FashionSense.CustomBody.Id", out var bodyId)) return null;
        if (TextureManagerInstance is null || GetSpecificAppearanceModelMethod?.Invoke(TextureManagerInstance, [bodyId]) is not { } bodyModel) return null;
        if (GetBodyFromFacingDirectionMethod?.Invoke(bodyModel, [who.FacingDirection]) is not { } body) return null;
        return body;
    }
}