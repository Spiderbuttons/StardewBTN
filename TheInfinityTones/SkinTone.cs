using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TheInfinityTones.Helpers;

namespace TheInfinityTones;

public struct SkinTone(Color darkest, Color medium, Color lightest) : IEquatable<SkinTone>
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
                for (int i = 0; i < data.Length; i += 3)
                {
                    Color darkest = data[i];
                    Color medium = data[i + 1];
                    Color lightest = data[i + 2];
                    field.Add(new SkinTone(darkest, medium, lightest));
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
    
    public static SkinTone GetSkinToneFromFarmer(Farmer who)
    {
        if (!who.modData.TryGetValue($"{ModEntry.Manifest.UniqueID}/SkinTone", out var skinToneString))
        {
            return SkinTone.VanillaSkinTones.ElementAtOrDefault(who.skin.Value);
        }
        return SkinTone.FromString(skinToneString);
    }

    public override string ToString()
    {
        return $"{Darkest.PackedValue} {Medium.PackedValue} {Lightest.PackedValue}";
    }
    
    public static SkinTone FromString(string str)
    {
        string[] parts = str.Split(' ');
        if (parts.Length != 3) throw new ArgumentException("Invalid skin tone string format.");
        
        Color darkest = new Color(uint.Parse(parts[0]));
        Color medium = new Color(uint.Parse(parts[1]));
        Color lightest = new Color(uint.Parse(parts[2]));
        
        return new SkinTone(darkest, medium, lightest);
    }

    public bool Equals(SkinTone other)
    {
        return Darkest.Equals(other.Darkest) && Medium.Equals(other.Medium) && Lightest.Equals(other.Lightest);
    }

    public override bool Equals(object? obj)
    {
        return obj is SkinTone other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Darkest, Medium, Lightest);
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