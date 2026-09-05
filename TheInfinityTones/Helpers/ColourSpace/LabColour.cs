using System;
using Microsoft.Xna.Framework;
// ReSharper disable MemberCanBePrivate.Global

namespace TheInfinityTones.Helpers.ColourSpace;

/// <summary>
/// Represents a colour in the CIE LAB colour space, with L, A, B, and alpha components relative to <see href="https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D">CIE Standard illuminant D65</see>.
/// </summary>
public readonly struct LabColour : IEquatable<LabColour>
{
    /// <summary>The maximum possible value for the L component of the colour.</summary>
    public const decimal MAX_L = 100M;
    /// <summary>The maximum possible value for the alpha (transparency) component of the colour.</summary>
    public const decimal MAX_ALPHA = 100M;
    
    /// <summary>
    /// The L component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal L;
    
    /// <summary>
    /// The A component of the colour.
    /// </summary>
    public readonly decimal A;
    
    /// <summary>
    /// The B component of the colour.
    /// </summary>
    public readonly decimal B;
    
    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Alpha;
    
    /// <summary>
    /// Initializes a new instance of an <see cref="LabColour"/> struct with the specified <paramref name="L"/>, <paramref name="A"/>, <paramref name="B"/>, and optional <paramref name="Alpha"/> values.
    /// </summary>
    /// <param name="L">The L component of the colour, ranging from 0 to 100.</param>
    /// <param name="A">The A component of the colour.</param>
    /// <param name="B">The B component of the colour.</param>
    /// <param name="Alpha">The alpha (transparency) component of the colour, ranging from 0 to 100. Defaults to 100 (fully opaque).</param>
    /// <remarks>The <paramref name="L"/> and <paramref name="Alpha"/> components will be clamped to the range [0, 100].</remarks>
    public LabColour(decimal L, decimal A, decimal B, decimal Alpha = MAX_ALPHA)
    {
        this.L = Math.Clamp(L, 0M, MAX_L);
        this.A = A;
        this.B = B;
        this.Alpha = Math.Clamp(Alpha, 0M, MAX_ALPHA);
    }

    /// <summary>
    /// Creates a <see cref="LabColour"/> from an <see cref="RgbColour"/>.
    /// </summary>
    /// <param name="rgb">The <see cref="RgbColour"/> to convert to a <see cref="LabColour"/>.</param>
    /// <returns>A <see cref="LabColour"/> representing the same colour as the provided <see cref="RgbColour"/>.</returns>
    public static LabColour FromRgb(RgbColour rgb)
    {
        return rgb.ToLab();
    }
    
    /// <summary>
    /// Creates a <see cref="LabColour"/> from an <see cref="HsvColour"/>.
    /// </summary>
    /// <param name="hsv">The <see cref="HsvColour"/> to convert to a <see cref="LabColour"/>.</param>
    /// <returns>A <see cref="LabColour"/> representing the same colour as the provided <see cref="HsvColour"/>.</returns>
    public static LabColour FromHsv(HsvColour hsv)
    {
        return RgbColour.FromHsv(hsv).ToLab();
    }

    /// <summary>
    /// Creates a <see cref="LabColour"/> from an <see cref="XyzColour"/>.
    /// </summary>
    /// <param name="xyz">The <see cref="XyzColour"/> to convert to a <see cref="LabColour"/>.</param>
    /// <returns>A <see cref="LabColour"/> representing the same colour as the provided <see cref="XyzColour"/>.</returns>
    public static LabColour FromXyz(XyzColour xyz)
    {
        return xyz.ToLab();
    }

    /// <summary>
    /// Creates a <see cref="LabColour"/> from a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <param name="color">The <see cref="Microsoft.Xna.Framework.Color"/> to convert to a <see cref="LabColour"/>.</param>
    /// <returns>A <see cref="LabColour"/> representing the same colour as the provided <see cref="Microsoft.Xna.Framework.Color"/>.</returns>
    public static LabColour FromXnaColor(Color color)
    {
        return RgbColour.FromXnaColor(color).ToLab();
    }
    
    /// <summary>
    /// Creates a <see cref="LabColour"/> from a hexadecimal colour string.
    /// </summary>
    /// <param name="hex">The hexadecimal colour string to convert to a <see cref="LabColour"/>. It can be in the format "#RRGGBB" or "#RRGGBBAA", with or without a leading "#".</param>
    /// <returns>A <see cref="LabColour"/> representing the same colour as the provided hexadecimal string.</returns>
    public static LabColour FromHexString(string hex)
    {
        return RgbColour.FromHexString(hex).ToLab();
    }
    
    /// <summary>
    /// Converts this <see cref="LabColour"/> to an <see cref="RgbColour"/>.
    /// </summary>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as this <see cref="LabColour"/>.</returns>
    public RgbColour ToRgb()
    {
        return ToXyz().ToRgb();
    }
    
    /// <summary>
    /// Converts this <see cref="LabColour"/> to an <see cref="HsvColour"/>.
    /// </summary>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as this <see cref="LabColour"/>.</returns>
    public HsvColour ToHsv()
    {
        return ToRgb().ToHsv();
    }

    /// <summary>
    /// Converts this <see cref="LabColour"/> to an <see cref="XyzColour"/>.
    /// </summary>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as this <see cref="LabColour"/>.</returns>
    public XyzColour ToXyz()
    {
        // https://en.wikipedia.org/wiki/CIELAB_color_space#Converting_between_CIELAB_and_CIE_XYZ_coordinates
        decimal y = (L + 16M) / 116M;
        decimal x = A / 500M + y;
        decimal z = y - B / 200M;

        decimal x3 = x * x * x;
        decimal y3 = y * y * y;
        decimal z3 = z * z * z;

        x = x3 > 0.008856M ? x3 : (x - 16M / 116M) / 7.787037M;
        y = y3 > 0.008856M ? y3 : (y - 16M / 116M) / 7.787037M;
        z = z3 > 0.008856M ? z3 : (z - 16M / 116M) / 7.787037M;
        
        return new XyzColour(
            X: x * XyzColour.MAX_X,
            Y: y * XyzColour.MAX_Y,
            Z: z * XyzColour.MAX_Z,
            Alpha: Alpha
        );
    }

    /// <summary>
    /// Converts this <see cref="LabColour"/> to a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <returns>A <see cref="Microsoft.Xna.Framework.Color"/> representing the same colour as this <see cref="LabColour"/>.</returns>
    public Color ToXnaColor()
    {
        return ToRgb().ToXnaColor();
    }
    
    /// <summary>
    /// Converts this <see cref="LabColour"/> to a hexadecimal colour string.
    /// </summary>
    /// <param name="includeAlpha">If true, the resulting string will include the alpha component; otherwise, it will only include the red, green, and blue components.</param>
    /// <returns>A hexadecimal colour string representing the same colour as this <see cref="LabColour"/> <b>without</b> a leading "#".</returns>
    public string ToHexString(bool includeAlpha = true)
    {
        return ToRgb().ToHexString(includeAlpha);
    }
    
    public static bool operator ==(LabColour lhs, LabColour rhs)
    {
        return lhs.L == rhs.L && lhs.A == rhs.A && lhs.B == rhs.B && lhs.Alpha == rhs.Alpha;
    }

    public static bool operator !=(LabColour lhs, LabColour rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(LabColour other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is LabColour other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (L + ((int)A << 8) + ((int)B << 16) + ((int)Alpha << 24)).GetHashCode();
    }
    
    /// <summary>
    /// Returns a string representation of the <see cref="LabColour"/> in the format:
    /// {L: <see cref="L" />, A: <see cref="A" />, B: <see cref="B" />, Alpha: <see cref="Alpha" />}.
    /// </summary>
    /// <returns>A string representation of the <see cref="LabColour"/>.</returns>
    public override string ToString()
    {
        return $"{{L: {L}, A: {A}, B: {B}, Alpha: {Alpha}}}";
    }
}