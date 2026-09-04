using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace TheInfinityTones.Patches;

[HarmonyPatch]
public static class FarmerRendererPatches
{
    [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.ApplySkinColor)), HarmonyPostfix]
    private static void FarmerRenderer_ApplySkinColor_Postfix(FarmerRenderer __instance, string texture_name, Color[] pixels)
    {
        if (ModEntry.StoredSkinTone.Value is not null)
        {
            __instance._SwapColor(texture_name, pixels, 260, ModEntry.GetStoredSkinColor(0));
            __instance._SwapColor(texture_name, pixels, 261, ModEntry.GetStoredSkinColor(1));
            __instance._SwapColor(texture_name, pixels, 262, ModEntry.GetStoredSkinColor(2));
            return;
        }
            
        Farmer? farmer = ModEntry.GetContextualFarmers().FirstOrDefault(f => f.FarmerRenderer == __instance);
        if (farmer is null) return;
            
        SkinTone skinTone = SkinTone.GetSkinToneFromFarmer(farmer);
        __instance._SwapColor(texture_name, pixels, 260, skinTone.Darkest);
        __instance._SwapColor(texture_name, pixels, 261, skinTone.Medium);
        __instance._SwapColor(texture_name, pixels, 262, skinTone.Lightest);
    }
}