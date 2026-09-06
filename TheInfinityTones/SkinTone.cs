using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TheInfinityTones.Helpers;
using TheInfinityTones.Helpers.ColourSpace;

namespace TheInfinityTones;

public struct SkinTone(Color darkest, Color medium, Color lightest, bool isDark) : IEquatable<SkinTone>
{
    public static List<SkinTone> VanillaSkinTones
    {
        get
        {
            if (field is not null) return field;
                
            field = [];
            try
            {
                Texture2D skinToneTexture = Game1.content.Load<Texture2D>("Characters/Farmer/skinColors");
                Color[] data = new Color[skinToneTexture.Width * skinToneTexture.Height];
                skinToneTexture.GetData(data);
                int[] darkSkinIndices = [4, 5, 6, 8, 14];
                for (int i = 0; i < data.Length; i += 3)
                {
                    Color darkest = data[i];
                    Color medium = data[i + 1];
                    Color lightest = data[i + 2];
                    int skinIndex = i / 3;
                    bool isDark = darkSkinIndices.Contains(skinIndex);
                    field.Add(new SkinTone(darkest, medium, lightest, isDark));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load vanilla skin tones: {ex}");
            }

            return field;
        }
        set;
    }
    
    public Color Darkest = darkest;
    public Color Medium = medium;
    public Color Lightest = lightest;
    public readonly bool IsDarkSkin = isDark;
    
    public static SkinTone GetSkinToneFromFarmer(Farmer who)
    {
        if (!who.modData.TryGetValue(ModEntry.MOD_DATA_KEY, out var skinToneString))
        {
            return VanillaSkinTones.ElementAtOrDefault(who.skin.Value);
        }
        return FromString(skinToneString);
    }

    public (SkinTone, int) GetClosestVanillaSkinTone()
    {
        LabColour thisLab = LabColour.FromXnaColor(Lightest);
        int closestIndex = -1;
        decimal closestDistance = decimal.MaxValue;

        for (int i = 0; i < VanillaSkinTones.Count; i++)
        {
            SkinTone vanillaTone = VanillaSkinTones[i];
            LabColour vanillaLab = LabColour.FromXnaColor(vanillaTone.Lightest);
            decimal distance = thisLab.CIEDE2000(vanillaLab);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return (VanillaSkinTones[closestIndex], closestIndex);
    }

    public override string ToString()
    {
        return $"{Darkest.PackedValue} {Medium.PackedValue} {Lightest.PackedValue} {IsDarkSkin}";
    }
    
    public static SkinTone FromString(string str)
    {
        string[] parts = str.Split(' ');
        if (parts.Length < 3) throw new ArgumentException("Invalid skin tone string format.");
        
        Color darkest = new Color(uint.Parse(parts[0]));
        Color medium = new Color(uint.Parse(parts[1]));
        Color lightest = new Color(uint.Parse(parts[2]));
        bool isDark = parts.Length > 3 && bool.Parse(parts[3]);
        
        return new SkinTone(darkest, medium, lightest, isDark);
    }

    public bool Equals(SkinTone other)
    {
        return Darkest.Equals(other.Darkest) && Medium.Equals(other.Medium) && Lightest.Equals(other.Lightest) && IsDarkSkin == other.IsDarkSkin;
    }

    public override bool Equals(object? obj)
    {
        return obj is SkinTone other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Darkest, Medium, Lightest, IsDarkSkin);
    }

    public static bool operator ==(SkinTone left, SkinTone right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SkinTone left, SkinTone right)
    {
        return !left.Equals(right);
    }
}