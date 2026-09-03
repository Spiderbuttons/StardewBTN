using System;
using Microsoft.Xna.Framework;

namespace TheInfinityTones.Helpers;

public struct RgbColour : IEquatable<RgbColour>
{
    /// <summary>
    /// The red component of the colour, ranging from 0 to 255.
    /// </summary>
    public decimal Red;
    
    /// <summary>
    /// The green component of the colour, ranging from 0 to 255.
    /// </summary>
    public decimal Green;
    
    /// <summary>
    /// The blue component of the colour, ranging from 0 to 255.
    /// </summary>
    public decimal Blue;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 255.
    /// </summary>
    public decimal Alpha;
    
    public decimal R => Red;
    public decimal G => Green;
    public decimal B => Blue;
    public decimal A => Alpha;
    
    public static bool operator==(RgbColour lhs, RgbColour rhs)
    {
        return lhs.Red == rhs.Red && lhs.Green == rhs.Green && lhs.Blue == rhs.Blue && lhs.Alpha == rhs.Alpha;
    }
    
    public static bool operator!=(RgbColour lhs, RgbColour rhs)
    {
        return !(lhs == rhs);
    }

    public RgbColour(decimal R, decimal G, decimal B, decimal A = 255M)
    {
        if (R is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(R), "Red value must be between 0 and 255.");
        if (G is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(G), "Green value must be between 0 and 255.");
        if (B is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(B), "Blue value must be between 0 and 255.");
        if (A is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(A), "Alpha value must be between 0 and 255.");
        Red = R;
        Green = G;
        Blue = B;
        Alpha = A;
    }

    public static RgbColour FromHsv(HsvColour hsv)
    {
        return hsv.ToRgb();
    }

    public static RgbColour FromXnaColor(Color color)
    {
        return new RgbColour(color.R, color.G, color.B, color.A);
    }
    
    public static RgbColour FromHexString(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentException("Hex string cannot be null or whitespace.", nameof(hex));
        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length is not 6 and not 8) throw new ArgumentException("Hex string must be 6 or 8 characters long.", nameof(hex));

        decimal r = Convert.ToInt32(hex.Substring(0, 2), 16);
        decimal g = Convert.ToInt32(hex.Substring(2, 2), 16);
        decimal b = Convert.ToInt32(hex.Substring(4, 2), 16);
        decimal a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) : 255;

        return new RgbColour(r, g, b, a);
    }

    public Color ToXnaColor()
    {
        return new Color((int)Math.Round(Red), (int)Math.Round(Green), (int)Math.Round(Blue), (int)Math.Round(Alpha));
    }

    public HsvColour ToHsv()
    {
        decimal min, max, delta;

        decimal r = Red / 255;
        decimal g = Green / 255;
        decimal b = Blue / 255;
        decimal a = Alpha / 255;

        decimal h, s, v;

        min = Math.Min(Math.Min(r, g), b);
        max = Math.Max(Math.Max(r, g), b);
        v = max;
        delta = max - min;

        if (max is 0 || delta is 0)
        {
            s = h = 0;
        }
        else
        {
            s = delta / max;
            if (r == max) h = (g - b) / delta;
            else if (g == max) h = 2 + (b - r) / delta;
            else h = 4 + (r - g) / delta;
        }
        
        h *= 60;
        if (h < 0) h += 360;
        
        return new HsvColour(
            H: h,
            S: s * 100,
            V: v * 100,
            A: a * 100
        );
    }

    public string ToHexString(bool includeAlpha = true)
    {
        return $"{(int)Math.Round(Red):X2}" +
               $"{(int)Math.Round(Green):X2}" +
               $"{(int)Math.Round(Blue):X2}" +
               $"{(includeAlpha ? $"{(int)Math.Round(Alpha):X2}" : "")}";
    }
    
    public bool Equals(RgbColour other)
    {
        return this == other;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is RgbColour other)
        {
            return this == other;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return (Red + ((int)Green << 8) + ((int)Blue << 16) + ((int)Alpha << 24)).GetHashCode();
    }

    public override string ToString()
    {
        return $"R: {Red}, G: {Green}, B: {Blue}, A: {Alpha}";
    }
}

public struct HsvColour : IEquatable<HsvColour>
{
    /// <summary>
    /// The hue of the colour, ranging from 0 to 360.
    /// </summary>
    public decimal Hue;
    
    /// <summary>
    /// The saturation of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Saturation;
    
    /// <summary>
    /// The value of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Value;

    /// <summary>
    /// The alpha (transparency) of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Alpha;
    
    public decimal H => Hue;
    public decimal S => Saturation;
    public decimal V => Value;
    public decimal A => Alpha;

    public static bool operator==(HsvColour lhs, HsvColour rhs)
    {
        return lhs.Hue == rhs.Hue && lhs.Saturation == rhs.Saturation && lhs.Value == rhs.Value && lhs.Alpha == rhs.Alpha;
    }
    
    public static bool operator!=(HsvColour lhs, HsvColour rhs)
    {
        return !(lhs == rhs);
    }
    
    public HsvColour(decimal H, decimal S, decimal V, decimal A = 100M)
    {
        if (H is < 0 or > 360) throw new ArgumentOutOfRangeException(nameof(H), "Hue value must be between 0 and 360.");
        if (S is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(S), "Saturation value must be between 0 and 100.");
        if (V is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(V), "Value must be between 0 and 100.");
        if (A is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(A), "Alpha must be between 0 and 100.");
        Hue = H;
        Saturation = S;
        Value = V;
        Alpha = A;
    }

    public static HsvColour FromXnaColor(Color color)
    {
        RgbColour rgb = new RgbColour(color.R, color.G, color.B, color.A);
        return rgb.ToHsv();
    }

    public Color ToXnaColor()
    {
        RgbColour rgb = ToRgb();
        return rgb.ToXnaColor();
    }

    public RgbColour ToRgb()
    {
        decimal r, g, b;

        decimal h = Hue % 360;
        decimal s = Saturation / 100;
        decimal v = Value / 100;
        decimal a = Alpha / 100;

        if (s is 0)
        {
            r = g = b = v;
        }
        else
        {
            decimal sectorPos = h / 60;
            decimal sectorNumber = Math.Floor(sectorPos);

            decimal fractionalSector = sectorPos - sectorNumber;

            decimal p = v * (1 - s);
            decimal q = v * (1 - s * fractionalSector);
            decimal t = v * (1 - s * (1 - fractionalSector));

            switch (sectorNumber)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;

                default:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
        }
        
        return new RgbColour(
            R: r * 255,
            G: g * 255,
            B: b * 255,
            A: a * 255
        );
    }

    public string ToHexString(bool includeAlpha = true)
    {
        RgbColour rgb = ToRgb();
        return $"#{(int)Math.Round(rgb.Red):X2}{(int)Math.Round(rgb.Green):X2}{(int)Math.Round(rgb.Blue):X2}{(includeAlpha ? $"{(int)Math.Round(rgb.Alpha):X2}" : "")}";
    }
    
    public bool Equals(HsvColour other)
    {
        return this == other;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is HsvColour other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (Hue + ((int)Saturation << 8) + ((int)Value << 16) + ((int)Alpha << 24)).GetHashCode();
    }
    
    public override string ToString()
    {
        return $"H: {Hue}, S: {Saturation}, V: {Value}, A: {Alpha}";
    }
}