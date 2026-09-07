using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace TheInfinityTones.Apis;

public interface IFashionSense
{
    public enum Type
    {
        Unknown,
        Hair,
        Accessory,
        [Obsolete("No longer maintained. Use Accessory instead.")]
        AccessorySecondary,
        [Obsolete("No longer maintained. Use Accessory instead.")]
        AccessoryTertiary,
        Hat,
        Shirt,
        Pants,
        Sleeves,
        Shoes,
        Player
    }
    
    KeyValuePair<bool, string> GetCurrentAppearanceId(Type appearanceType, Farmer? target = null);
    KeyValuePair<bool, string> ResetAppearanceTexture(string appearanceId, IManifest callerManifest);
}