using System;
using Microsoft.Xna.Framework;
// ReSharper disable MemberCanBePrivate.Global

namespace TheInfinityTones.Helpers.ColourSpace;

/// <summary>
/// Represents a colour in the XYZ colour space, with X, Y, Z, and alpha components relative to <see href="https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D">CIE Standard illuminant D65</see>.
/// </summary>
public readonly struct XyzColour : IEquatable<XyzColour>
{
    /// <summary>The maximum possible value for the X component of the colour.</summary>
    public const decimal MAX_X = 95.047M;
    /// <summary>The maximum possible value for the Y component of the colour.</summary>
    public const decimal MAX_Y = 100M;
    /// <summary>The maximum possible value for the Z component of the colour.</summary>
    public const decimal MAX_Z = 108.883M;
    /// <summary>The maximum possible value for the alpha (transparency) component of the colour.</summary>
    public const decimal MAX_ALPHA = 100M;
    
    /// <summary>
    /// The X component of the colour, ranging from 0 to 95.0489.
    /// </summary>
    public readonly decimal X;

    /// <summary>
    /// The Y component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Y;

    /// <summary>
    /// The Z component of the colour, ranging from 0 to 108.884.
    /// </summary>
    public readonly decimal Z;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Alpha;

    /// <inheritdoc cref="A" />
    public decimal A => Alpha;

    /// <summary>
    /// Initializes a new instance of an <see cref="XyzColour"/> struct with the specified <paramref name="X"/>, <paramref name="Y"/>, <paramref name="Z"/>, and optional <paramref name="Alpha"/> values.
    /// </summary>
    /// <param name="X">The X component of the colour, ranging from 0 to 95.0489.</param>
    /// <param name="Y">The Y component of the colour, ranging from 0 to 100.</param>
    /// <param name="Z">The Z component of the colour, ranging from 0 to 108.884.</param>
    /// <param name="Alpha">The alpha (transparency) component of the colour, ranging from 0 to 100. Defaults to 100 (fully opaque).</param>
    /// <remarks>
    ///     <para>
    ///         The <paramref name="X"/>, <paramref name="Y"/>, <paramref name="Z"/>, and <paramref name="Alpha"/> components will be clamped to the following ranges:
    ///         <br/><paramref name="X"/>: [0, 95.047]<br/>
    ///         <paramref name="Y"/>: [0, 100]<br/>
    ///         <paramref name="Z"/>: [0, 108.883]<br/>
    ///         <paramref name="Alpha"/>: [0, 100]
    ///     </para>
    /// </remarks>
    public XyzColour(decimal X, decimal Y, decimal Z, decimal Alpha = MAX_ALPHA)
    {
        this.X = Math.Clamp(X, 0M, MAX_X);
        this.Y = Math.Clamp(Y, 0M, MAX_Y);
        this.Z = Math.Clamp(Z, 0M, MAX_Z);
        this.Alpha = Math.Clamp(Alpha, 0M, MAX_ALPHA);
    }

    /// <summary>
    /// Creates an <see cref="XyzColour"/> from an <see cref="RgbColour"/>.
    /// </summary>
    /// <param name="rgb">The <see cref="RgbColour"/> to convert to an <see cref="XyzColour"/>.</param>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as the provided <see cref="RgbColour"/>.</returns>
    public static XyzColour FromRgb(RgbColour rgb)
    {
        return rgb.ToXyz();
    }

    /// <summary>
    /// Creates an <see cref="XyzColour"/> from an <see cref="HsvColour"/>.
    /// </summary>
    /// <param name="hsv">The <see cref="HsvColour"/> to convert to an <see cref="XyzColour"/>.</param>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as the provided <see cref="HsvColour"/>.</returns>
    public static XyzColour FromHsv(HsvColour hsv)
    {
        return RgbColour.FromHsv(hsv).ToXyz();
    }
    
    /// <summary>
    /// Creates an <see cref="XyzColour"/> from a <see cref="LabColour"/>.
    /// </summary>
    /// <param name="lab">The <see cref="LabColour"/> to convert to an <see cref="XyzColour"/>.</param>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as the provided <see cref="LabColour"/>.</returns>
    public static XyzColour FromLab(LabColour lab)
    {
        return RgbColour.FromLab(lab).ToXyz();
    }
    
    public static XyzColour FromLch(LchColour lch)
    {
        return RgbColour.FromLch(lch).ToXyz();
    }

    /// <summary>
    /// Creates an <see cref="XyzColour"/> from a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <param name="color">The <see cref="Microsoft.Xna.Framework.Color"/> to convert to an <see cref="XyzColour"/>.</param>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as the provided <see cref="Microsoft.Xna.Framework.Color"/>.</returns>
    public static XyzColour FromXnaColor(Color color)
    {
        return RgbColour.FromXnaColor(color).ToXyz();
    }

    /// <summary>
    /// Creates an <see cref="XyzColour"/> from a hexadecimal colour string.
    /// </summary>
    /// <param name="hex">The hexadecimal colour string to convert to an <see cref="XyzColour"/>. It can be in the format "#RRGGBB" or "#RRGGBBAA", with or without a leading "#".</param>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as the provided hexadecimal string.</returns>
    public static XyzColour FromHexString(string hex)
    {
        return RgbColour.FromHexString(hex).ToXyz();
    }

    /// <summary>
    /// Converts this <see cref="XyzColour"/> to an <see cref="RgbColour"/>.
    /// </summary>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as this <see cref="XyzColour"/>.</returns>
    public RgbColour ToRgb()
    {
        decimal x = X / 100;
        decimal y = Y / 100;
        decimal z = Z / 100;
        decimal a = Alpha / 100;

        // See http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html
        decimal r = x *  3.2404542M + y * -1.5371385M + z * -0.4985314M;
        decimal g = x * -0.9692660M + y *  1.8760108M + z *  0.0415560M;
        decimal b = x *  0.0556434M + y * -0.2040259M + z *  1.0572252M;

        // See https://en.wikipedia.org/wiki/SRGB#Transfer_function_(%22gamma%22)
        r = r > 0.0031308M ? (decimal)(1.055 * Math.Pow((double)r, 1 / 2.4) - 0.055) : r * 12.92M;
        g = g > 0.0031308M ? (decimal)(1.055 * Math.Pow((double)g, 1 / 2.4) - 0.055) : g * 12.92M;
        b = b > 0.0031308M ? (decimal)(1.055 * Math.Pow((double)b, 1 / 2.4) - 0.055) : b * 12.92M;

        return new RgbColour(
            R: Math.Round(Math.Clamp(r * RgbColour.MAX_RED, 0, RgbColour.MAX_RED)),
            G: Math.Round(Math.Clamp(g * RgbColour.MAX_GREEN, 0, RgbColour.MAX_GREEN)),
            B: Math.Round(Math.Clamp(b * RgbColour.MAX_BLUE, 0, RgbColour.MAX_BLUE)),
            A: Math.Round(Math.Clamp(a * RgbColour.MAX_ALPHA, 0, RgbColour.MAX_ALPHA))
        );
    }

    /// <summary>
    /// Converts this <see cref="XyzColour"/> to an <see cref="HsvColour"/>.
    /// </summary>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as this <see cref="XyzColour"/>.</returns>
    public HsvColour ToHsv()
    {
        return ToRgb().ToHsv();
    }

    /// <summary>
    /// Converts this <see cref="XyzColour"/> to a <see cref="LabColour"/>.
    /// </summary>
    /// <returns>A <see cref="LabColour"/> representing the same colour as this <see cref="XyzColour"/>.</returns>
    public LabColour ToLab()
    {
        // https://en.wikipedia.org/wiki/CIELAB_color_space#Converting_between_CIELAB_and_CIE_XYZ_coordinates
        decimal x = X / MAX_X;
        decimal y = Y / MAX_Y;
        decimal z = Z / MAX_Z;
        
        x = x > 0.008856M ? (decimal)Math.Pow((double)x, 1.0 / 3.0) : 7.787037M * x + 16M / 116M;
        y = y > 0.008856M ? (decimal)Math.Pow((double)y, 1.0 / 3.0) : 7.787037M * y + 16M / 116M;
        z = z > 0.008856M ? (decimal)Math.Pow((double)z, 1.0 / 3.0) : 7.787037M * z + 16M / 116M;
        
        return new LabColour(
            L: 116M * y - 16M,
            A: 500M * (x - y),
            B: 200M * (y - z),
            Alpha: Alpha
        );
    }

    /// <summary>
    /// Converts this <see cref="XyzColour"/> to a <see cref="LchColour"/>.
    /// </summary>
    /// <returns>A <see cref="LchColour"/> representing the same colour as this <see cref="XyzColour"/>.</returns>
    public LchColour ToLch()
    {
        return ToRgb().ToLch();
    }

    /// <summary>
    /// Converts this <see cref="XyzColour"/> to a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <returns>A <see cref="Microsoft.Xna.Framework.Color"/> representing the same colour as this <see cref="XyzColour"/>.</returns>
    public Color ToXnaColor()
    {
        return ToRgb().ToXnaColor();
    }

    /// <summary>
    /// Converts this <see cref="XyzColour"/> to a hexadecimal colour string.
    /// </summary>
    /// <param name="includeAlpha">If true, the resulting string will include the alpha component; otherwise, it will only include the red, green, and blue components.</param>
    /// <returns>A hexadecimal colour string representing the same colour as this <see cref="XyzColour"/> <b>without</b> a leading "#".</returns>
    public string ToHexString(bool includeAlpha = true)
    {
        return ToRgb().ToHexString(includeAlpha);
    }

    public static bool operator ==(XyzColour lhs, XyzColour rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z && lhs.Alpha == rhs.Alpha;
    }

    public static bool operator !=(XyzColour lhs, XyzColour rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(XyzColour other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is XyzColour other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (X + ((int)Y << 8) + ((int)Z << 16) + ((int)Alpha << 24)).GetHashCode();
    }
    
    /// <summary>
    /// Returns a string representation of the <see cref="XyzColour"/> in the format:
    /// {X: <see cref="X" />, Y: <see cref="Y" />, Z: <see cref="Z" />, A: <see cref="A" />}.
    /// </summary>
    /// <returns>A string representation of the <see cref="XyzColour"/>.</returns>
    public override string ToString()
    {
        return $"{{X: {X}, Y: {Y}, Z: {Z}, A: {Alpha}}}";
    }
}