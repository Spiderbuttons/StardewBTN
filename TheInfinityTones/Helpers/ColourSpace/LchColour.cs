using System;
using Microsoft.Xna.Framework;
// ReSharper disable MemberCanBePrivate.Global

namespace TheInfinityTones.Helpers.ColourSpace;

/// <summary>
/// Represents a colour in the CIE Lch(ab) colour space, with L, C, H, and alpha components relative to <see href="https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D">CIE Standard illuminant D65</see>.
/// <remarks>Not to be confused with the <see href="https://en.wikipedia.org/wiki/CIELUV#Cylindrical_representation_(CIELCh)">Lch(uv) colour space.</see></remarks>
/// </summary>
public readonly struct LchColour : IEquatable<LchColour>
{
    /// <summary>
    /// The L component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Lightness;

    /// <summary>
    /// The Chroma component of the colour.
    /// </summary>
    public readonly decimal Chroma;

    /// <summary>
    /// The Hue component of the colour, ranging from 0 to 360.
    /// </summary>
    public readonly decimal Hue;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Alpha;
    
    /// <inheritdoc cref="Lightness"/>
    public decimal L => Lightness;
    
    /// <inheritdoc cref="Chroma"/>
    public decimal C => Chroma;
    
    /// <inheritdoc cref="Hue"/>
    public decimal H => Hue;

    /// <summary>
    /// Initializes a new instance of an <see cref="LchColour"/> struct with the specified <paramref name="L"/>, <paramref name="C"/>, <paramref name="H"/>, and optional <paramref name="Alpha"/> values.
    /// </summary>
    /// <param name="L">The lightness component of the colour, ranging from 0 to 100.</param>
    /// <param name="C">The chroma component of the colour.</param>
    /// <param name="H">The hue component of the colour, ranging from 0 to 360.</param>
    /// <param name="Alpha">The alpha (transparency) component of the colour, ranging from 0 to 100. Defaults to 100 (fully opaque).</param>
    /// <remarks>The <paramref name="L"/> and <paramref name="Alpha"/> components will be clamped to the range [0, 100].</remarks>
    public LchColour(decimal L, decimal C, decimal H, decimal Alpha = 100M)
    {
        Lightness = Math.Clamp(L, 0M, 100M);
        Chroma = C;
        Hue = H;
        this.Alpha = Math.Clamp(Alpha, 0M, 100M);
    }
    
    /// <summary>
    /// Creates an <see cref="LchColour"/> from an <see cref="RgbColour"/>.
    /// </summary>
    /// <param name="rgb">The <see cref="RgbColour"/> to convert to an <see cref="LchColour"/>.</param>
    /// <returns>An <see cref="LchColour"/> representing the same colour as the provided <see cref="RgbColour"/>.</returns>
    public static LchColour FromRgb(RgbColour rgb)
    {
        return rgb.ToLch();
    }
    
    /// <summary>
    /// Creates an <see cref="LchColour"/> from an <see cref="HsvColour"/>.
    /// </summary>
    /// <param name="hsv">The <see cref="HsvColour"/> to convert to an <see cref="LchColour"/>.</param>
    /// <returns>An <see cref="LchColour"/> representing the same colour as the provided <see cref="HsvColour"/>.</returns>
    public static LchColour FromHsv(HsvColour hsv)
    {
        return RgbColour.FromHsv(hsv).ToLch();
    }
    
    /// <summary>
    /// Creates an <see cref="LchColour"/> from an <see cref="XyzColour"/>.
    /// </summary>
    /// <param name="xyz">The <see cref="XyzColour"/> to convert to an <see cref="LchColour"/>.</param>
    /// <returns>An <see cref="LchColour"/> representing the same colour as the provided <see cref="XyzColour"/>.</returns>
    public static LchColour FromXyz(XyzColour xyz)
    {
        return RgbColour.FromXyz(xyz).ToLch();
    }
    
    /// <summary>
    /// Creates an <see cref="LchColour"/> from an <see cref="LabColour"/>.
    /// </summary>
    /// <param name="lab">The <see cref="LabColour"/> to convert to an <see cref="LchColour"/>.</param>
    /// <returns>An <see cref="LchColour"/> representing the same colour as the provided <see cref="LabColour"/>.</returns>
    public static LchColour FromLab(LabColour lab)
    {
        return RgbColour.FromLab(lab).ToLch();
    }
    
    /// <summary>
    /// Creates an <see cref="LchColour"/> from a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <param name="color">The <see cref="Microsoft.Xna.Framework.Color"/> to convert to an <see cref="LchColour"/>.</param>
    /// <returns>An <see cref="LchColour"/> representing the same colour as the provided <see cref="Microsoft.Xna.Framework.Color"/>.</returns>
    public static LchColour FromXnaColor(Color color)
    {
        return RgbColour.FromXnaColor(color).ToLch();
    }
    
    /// <summary>
    /// Creates an <see cref="LchColour"/> from a hexadecimal colour string.
    /// </summary>
    /// <param name="hex">The hexadecimal colour string to convert to an <see cref="LchColour"/>. It can be in the format "#RRGGBB" or "#RRGGBBAA", with or without a leading "#".</param>
    /// <returns>An <see cref="LchColour"/> representing the same colour as the provided hexadecimal string.</returns>
    public static LchColour FromHexString(string hex)
    {
        return RgbColour.FromHexString(hex).ToLch();
    }

    /// <summary>
    /// Converts this <see cref="LchColour"/> to an <see cref="RgbColour"/>.
    /// </summary>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as this <see cref="LchColour"/>.</returns>
    public RgbColour ToRgb()
    {
        return ToLab().ToRgb();
    }
    
    /// <summary>
    /// Converts this <see cref="LchColour"/> to an <see cref="HsvColour"/>.
    /// </summary>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as this <see cref="LchColour"/>.</returns>
    public HsvColour ToHsv()
    {
        return ToRgb().ToHsv();
    }
    
    /// <summary>
    /// Converts this <see cref="LchColour"/> to an <see cref="XyzColour"/>.
    /// </summary>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as this <see cref="LchColour"/>.</returns>
    public XyzColour ToXyz()
    {
        return ToRgb().ToXyz();
    }

    /// <summary>
    /// Converts this <see cref="LchColour"/> to a <see cref="LabColour"/>.
    /// </summary>
    /// <returns>A <see cref="LabColour"/> representing the same colour as this <see cref="LchColour"/>.</returns>
    public LabColour ToLab()
    {
        decimal a = Chroma * DecimalCos(Hue);
        decimal b = Chroma * DecimalSin(Hue);
        return new LabColour(Lightness, a, b, Alpha);
        
        // https://stackoverflow.com/a/27235493
        decimal DecimalCos(decimal degrees)
        {
            decimal radians = degrees * (decimal)Math.PI / 180M;
            decimal cos = 1M;
            decimal term = 1M;
            for (int n = 1; n <= 10; n++)
            {
                term *= -radians * radians / (2 * n * (2 * n - 1));
                cos += term;
            }
            return cos;
        }
        
        decimal DecimalSin(decimal degrees)
        {
            decimal radians = degrees * (decimal)Math.PI / 180M;
            decimal sin = radians;
            decimal term = radians;
            for (int n = 1; n <= 10; n++)
            {
                term *= -radians * radians / (2 * n * (2 * n + 1));
                sin += term;
            }
            return sin;
        }
    }
    
    /// <summary>
    /// Converts this <see cref="LchColour"/> to a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <returns>A <see cref="Microsoft.Xna.Framework.Color"/> representing the same colour as this <see cref="LchColour"/>.</returns>
    public Color ToXnaColor()
    {
        return ToRgb().ToXnaColor();
    }
    
    /// <summary>
    /// Converts this <see cref="LchColour"/> to a hexadecimal colour string.
    /// </summary>
    /// <param name="includeAlpha">If true, the resulting string will include the alpha component; otherwise, it will only include the red, green, and blue components.</param>
    /// <returns>A hexadecimal colour string representing the same colour as this <see cref="LchColour"/>.</returns>
    public string ToHexString(bool includeAlpha = true)
    {
        return ToRgb().ToHexString(includeAlpha);
    }
    
    public static bool operator ==(LchColour lhs, LchColour rhs)
    {
        return lhs.Lightness == rhs.Lightness && lhs.Chroma == rhs.Chroma && lhs.Hue == rhs.Hue && lhs.Alpha == rhs.Alpha;
    }

    public static bool operator !=(LchColour lhs, LchColour rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(LchColour other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is LchColour other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (Lightness + ((int)Chroma << 8) + ((int)Hue << 16) + ((int)Alpha << 24)).GetHashCode();
    }
    
    /// <summary>
    /// Returns a string representation of the <see cref="LchColour"/> in the format:
    /// {L: <see cref="Lightness" />, C: <see cref="Chroma" />, H: <see cref="Hue" />, Alpha: <see cref="Alpha" />}.
    /// </summary>
    /// <returns>A string representation of the <see cref="LchColour"/>.</returns>
    public override string ToString()
    {
        return $"{{L: {Lightness}, C: {Chroma}, H: {Hue}, Alpha: {Alpha}}}";
    }
}