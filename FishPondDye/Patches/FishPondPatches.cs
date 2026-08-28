using System;
using FishPondDye.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using Object = StardewValley.Object;

namespace FishPondDye.Patches;

[HarmonyPatch(typeof(FishPond))]
public class FishPondPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(FishPond.performActiveObjectDropInAction))]
    public static void FishPond_performActiveObjectDropInAction_Postfix(FishPond __instance, Farmer who, bool probe, ref bool __result)
    {
        if (__result) return;
        if (who.ActiveObject is not { } heldObject) return;
        if (!heldObject.QualifiedItemId.StartsWith($"(O){ModEntry.UNIQUE_ID}")) return;

        if (probe)
        {
            __result = true;
            return;
        }
        
        who.reduceActiveItemByOne();

        Color targetColour;
        string[] idSplit = heldObject.QualifiedItemId.Split('_');
        if (idSplit.Length < 3) targetColour = new Color(60, 126, 150);
        else {
            Log.Debug($"Dye ID: {heldObject.QualifiedItemId}, Colour: {idSplit[2]}");
            targetColour = idSplit[2] switch 
            {
                "Red" => Color.Red,
                "Orange" => Color.Orange,
                "Green" => Color.Green,
                "Blue" => Color.Blue,
                "Purple" => Color.Purple,
                _ => new Color(60, 126, 150)
            };
        }
        
        __instance.showObjectThrownIntoPondAnimation(who, heldObject, () => dropDyeIntoPond(__instance, heldObject, targetColour));
        __result = true;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(FishPond.Update))]
    public static void FishPond_Update_Postfix(FishPond __instance)
    {
        if (GetColourFromModData(__instance, $"{ModEntry.UNIQUE_ID}/TargetColour") is { } targetColour)
        {
            Color startingColour = GetColourFromModData(__instance, $"{ModEntry.UNIQUE_ID}/StartingColour") ?? (__instance.GetWaterColor(Vector2.Zero) ?? __instance.GetParentLocation().waterColor.Value);

            double dyeTime = GetDoubleFromModData(__instance, $"{ModEntry.UNIQUE_ID}/DyeTime") ?? Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            
            double timeSinceDye = Game1.currentGameTime.TotalGameTime.TotalMilliseconds - dyeTime;
            double transitionDuration = 1000;
            float transitionProgress = (float)Math.Min(timeSinceDye / transitionDuration, 1.0);
            
            Color newColour = Color.Lerp(startingColour, targetColour, transitionProgress);

            __instance.overrideWaterColor.Value = newColour;
            if (newColour == targetColour && targetColour == new Color(60, 126, 150))
            {
                __instance.overrideWaterColor.Value = Color.White;
                __instance.modData.Remove($"{ModEntry.UNIQUE_ID}/TargetColour");
                __instance.modData.Remove($"{ModEntry.UNIQUE_ID}/CurrentColour");
                __instance.modData.Remove($"{ModEntry.UNIQUE_ID}/StartingColour");
                __instance.modData.Remove($"{ModEntry.UNIQUE_ID}/DyeTime");
            }
            else
            {
                __instance.modData[$"{ModEntry.UNIQUE_ID}/CurrentColour"] = $"{newColour.PackedValue}";
            }
        }
        
        if (GetColourFromModData(__instance, $"{ModEntry.UNIQUE_ID}/CurrentColour") is { } currentColour)
        {
            __instance.overrideWaterColor.Value = currentColour;
        }
    }
    
    private static void dropDyeIntoPond(FishPond pond, Object dye, Color color)
    {
        Log.Alert($"Dropping dye ({dye.QualifiedItemId}) into pond at {pond.tileX.Value}, {pond.tileY.Value}");
        
        pond.modData[$"{ModEntry.UNIQUE_ID}/DyeTime"] = $"{Game1.currentGameTime.TotalGameTime.TotalMilliseconds}";
        pond.modData[$"{ModEntry.UNIQUE_ID}/StartingColour"] = $"{(pond.GetWaterColor(Vector2.Zero) ?? new Color(60, 126, 150)).PackedValue}";
        pond.modData[$"{ModEntry.UNIQUE_ID}/TargetColour"] = $"{color.PackedValue}";
        
        Log.Info($"Target colour set to {color} ({color.PackedValue})");
    }

    private static Color? GetColourFromModData(IHaveModData? source, string key)
    {
        if (source?.modData.TryGetValue(key, out var packedColourString) == true &&
            uint.TryParse(packedColourString, out uint packedColour))
        {
            return new Color(packedColour);
        }
        return null;
    }
    
    private static double? GetDoubleFromModData(IHaveModData? source, string key)
    {
        if (source?.modData.TryGetValue(key, out var valueString) == true &&
            double.TryParse(valueString, out double value))
        {
            return value;
        }
        return null;
    }
}