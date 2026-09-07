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
    public static readonly List<SkinTone> VanillaSkinTones =
    [
        new(darkest: 0xFF3A006B, medium: 0xFF656BE0, lightest: 0xFF89AEF9, isDarkSkin: false),
        new(darkest: 0xFF352455, medium: 0xFF3D43A8, lightest: 0xFF668CE1, isDarkSkin: false),
        new(darkest: 0xFF352455, medium: 0xFF486ED1, lightest: 0xFF82A0F0, isDarkSkin: false),
        new(darkest: 0xFF393370, medium: 0xFF8795E3, lightest: 0xFF9AB9F7, isDarkSkin: false),
        new(darkest: 0xFF151D58, medium: 0xFF2D3F9A, lightest: 0xFF4764C4, isDarkSkin: true ),
        new(darkest: 0xFF121841, medium: 0xFF192A70, lightest: 0xFF395FAE, isDarkSkin: true ),
        new(darkest: 0xFF0B1342, medium: 0xFF142077, lightest: 0xFF1246A2, isDarkSkin: true ),
        new(darkest: 0xFF1A2066, medium: 0xFF2C5EBE, lightest: 0xFF3B8AD2, isDarkSkin: false),
        new(darkest: 0xFF182555, medium: 0xFF1A48A2, lightest: 0xFF4479BD, isDarkSkin: true ),
        new(darkest: 0xFF34397C, medium: 0xFF6E73C2, lightest: 0xFFB2ABFF, isDarkSkin: false),
        new(darkest: 0xFF4C3453, medium: 0xFF6C619A, lightest: 0xFFA9B2D6, isDarkSkin: false),
        new(darkest: 0xFF272155, medium: 0xFF2D40A3, lightest: 0xFF5E8AE8, isDarkSkin: false),
        new(darkest: 0xFF343654, medium: 0xFF5F8298, lightest: 0xFFB1E2E2, isDarkSkin: false),
        new(darkest: 0xFF352455, medium: 0xFF5A4EAC, lightest: 0xFFA08CEF, isDarkSkin: false),
        new(darkest: 0xFF040B2E, medium: 0xFF13285A, lightest: 0xFF29548C, isDarkSkin: true ),
        new(darkest: 0xFF312655, medium: 0xFF2940AF, lightest: 0xFF5483DA, isDarkSkin: false),
        new(darkest: 0xFF510600, medium: 0xFFCE7100, lightest: 0xFFE8BF68, isDarkSkin: false),
        new(darkest: 0xFF104103, medium: 0xFF16B240, lightest: 0xFF88E8BD, isDarkSkin: false),
        new(darkest: 0xFF03033F, medium: 0xFF1616AF, lightest: 0xFF8E8EFF, isDarkSkin: false),
        new(darkest: 0xFF561A3C, medium: 0xFFCE0078, lightest: 0xFFFF91B2, isDarkSkin: false),
        new(darkest: 0xFF03274E, medium: 0xFF1A85CC, lightest: 0xFF8CDDFF, isDarkSkin: false),
        new(darkest: 0xFF433C42, medium: 0xFF817C88, lightest: 0xFFD3D2DD, isDarkSkin: false),
        new(darkest: 0xFF3E0C79, medium: 0xFF7582FA, lightest: 0xFF9ECDFF, isDarkSkin: false),
        new(darkest: 0xFF322F72, medium: 0xFF879BFF, lightest: 0xFFB5D3FF, isDarkSkin: false)
    ];
    
    public Color Darkest = darkest;
    public Color Medium = medium;
    public Color Lightest = lightest;
    public readonly bool IsDarkSkin = isDark;

    public SkinTone(uint darkest, uint medium, uint lightest, bool isDarkSkin) : this(new Color(darkest), new Color(medium), new Color(lightest), isDarkSkin) { }
    
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