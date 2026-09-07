using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu;

namespace TheInfinityTones.Patches;

public static class FashionSensePatches
{
    private static ConstructorInfo SkinToneModelCtor = null!;
    private static MethodInfo? GetBodyColorMethod;
    private static MethodInfo? GetSpecificAppearanceModelMethod;
    private static MethodInfo? GetBodyFromFacingDirectionMethod;
    private static MethodInfo? ShouldHideLegsMethod;
    
    public static object? TextureManagerInstance;
    public static MethodInfo? SetSpriteDirtyMethod;
    
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
            harmony.Patch(
                original: AccessTools.Method(drawPatchType, "ApplyShoeColorPrefix"),
                prefix: new HarmonyMethod(methodType: typeof(FashionSensePatches), methodName: nameof(DrawPatch_ApplyShoeColorPrefix_Prefix))
            );
            
            GetBodyColorMethod = AccessTools.Method(drawPatchType, "GetBodyColor") ?? throw new Exception("Could not find GetBodyColor method.");
            
            Type fashionSenseType = AccessTools.TypeByName("FashionSense.FashionSense") ?? throw new Exception("Could not find FashionSense type.");
            TextureManagerInstance = AccessTools.Field(fashionSenseType, "textureManager")?.GetValue(null) ?? throw new Exception("Could not find TextureManager instance.");
            SetSpriteDirtyMethod = AccessTools.Method(fashionSenseType, "SetSpriteDirty") ?? throw new Exception("Could not find SetSpriteDirty method.");

            Type bodyContentPackType = AccessTools.TypeByName("FashionSense.Framework.Models.Appearances.Body.BodyContentPack") ?? throw new Exception("Could not find BodyContentPack type.");
            GetBodyFromFacingDirectionMethod = AccessTools.Method(bodyContentPackType, "GetBodyFromFacingDirection") ??
                                               throw new Exception("Could not find GetBodyFromFacingDirection method.");
            
            Type textureManagerType = AccessTools.TypeByName("FashionSense.Framework.Managers.TextureManager") ?? throw new Exception("Could not find TextureManager type.");
            GetSpecificAppearanceModelMethod = AccessTools.Method(textureManagerType, "GetSpecificAppearanceModel")?.MakeGenericMethod(bodyContentPackType) ??
                                               throw new Exception("Could not find GetSpecificAppearanceModel method.");
            
            Type appearanceHelpersType = AccessTools.TypeByName("FashionSense.Framework.Utilities.AppearanceHelpers") ?? throw new Exception("Could not find AppearanceHelpers type.");
            ShouldHideLegsMethod = AccessTools.Method(appearanceHelpersType, "ShouldHideLegs") ?? throw new Exception("Could not find ShouldHideLegs method.");
        } 
        catch (Exception ex)
        {
            Log.Error(message: $"Failed to patch Fashion Sense: {ex}");
        }
    }

    // If I don't do this then we won't have any boots in the farmer preview in the colour picker.
    // This will only draw vanilla boots, but that's better than no boots at all.
    public static bool DrawPatch_ApplyShoeColorPrefix_Prefix(object[] __args)
    {
        if ((TitleMenu.subMenu as ColourPickerMenu ?? Game1.activeClickableMenu as ColourPickerMenu) is null)
            return true;
        
        if (__args[0] is not FarmerRenderer renderer) return true;
        Farmer who = Game1.player;
        foreach (Farmer farmer in Game1.getOnlineFarmers())
        {
            if (farmer.FarmerRenderer == renderer)
            {
                who = farmer;
            }
        }
        
        if (who.modData.TryGetValue("FashionSense.CustomShoes.Id", out var val) && val == "Override Shoe Color" && ShouldHideLegsMethod?.Invoke(null, [who, who.FacingDirection]) is false)
        {
            return true;
        }

        return false;
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