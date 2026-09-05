using System;
using Microsoft.Xna.Framework;
// ReSharper disable MemberCanBePrivate.Global

namespace TheInfinityTones.Helpers.ColourSpace;

/// <summary>
/// Represents a colour in the RGB colour space, with red, green, blue, and alpha components.
/// </summary>
public readonly struct RgbColour : IEquatable<RgbColour>
{
    /// <summary>The maximum possible value for the red component of the colour.</summary>
    public const decimal MAX_RED = 255M;
    /// <summary>The maximum possible value for the green component of the colour.</summary>
    public const decimal MAX_GREEN = 255M;
    /// <summary>The maximum possible value for the blue component of the colour.</summary>
    public const decimal MAX_BLUE = 255M;
    /// <summary>The maximum possible value for the alpha (transparency) component of the colour.</summary>
    public const decimal MAX_ALPHA = 255M;
    
    /// <summary>
    /// The red component of the colour, ranging from 0 to 255.
    /// </summary>
    public readonly decimal Red;
    
    /// <summary>
    /// The green component of the colour, ranging from 0 to 255.
    /// </summary>
    public readonly decimal Green;
    
    /// <summary>
    /// The blue component of the colour, ranging from 0 to 255.
    /// </summary>
    public readonly decimal Blue;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 255.
    /// </summary>
    public readonly decimal Alpha;

    /// <inheritdoc cref="Red" />
    public decimal R => Red;
    
    /// <inheritdoc cref="Green" />
    public decimal G => Green;
    
    /// <inheritdoc cref="Blue" />
    public decimal B => Blue;
    
    /// <inheritdoc cref="Alpha" />
    public decimal A => Alpha;

    /// <summary>
    /// Initializes a new instance of an <see cref="RgbColour"/> struct with the specified <paramref name="R"/>, <paramref name="G"/>, <paramref name="B"/>, and optional <paramref name="A"/> values.
    /// </summary>
    /// <param name="R">The red component of the colour, ranging from 0 to 255.</param>
    /// <param name="G">The green component of the colour, ranging from 0 to 255.</param>
    /// <param name="B">The blue component of the colour, ranging from 0 to 255.</param>
    /// <param name="A">The alpha (transparency) component of the colour, ranging from 0 to 255. Defaults to 255 (fully opaque).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the colour components are outside their valid ranges.</exception>
    public RgbColour(decimal R, decimal G, decimal B, decimal A = MAX_ALPHA)
    {
        if (R is < 0 or > MAX_RED) throw new ArgumentOutOfRangeException(nameof(R), $"Red value must be between 0 and {MAX_RED}.");
        if (G is < 0 or > MAX_GREEN) throw new ArgumentOutOfRangeException(nameof(G), $"Green value must be between 0 and {MAX_GREEN}.");
        if (B is < 0 or > MAX_BLUE) throw new ArgumentOutOfRangeException(nameof(B), $"Blue value must be between 0 and {MAX_BLUE}.");
        if (A is < 0 or > MAX_ALPHA) throw new ArgumentOutOfRangeException(nameof(A), $"Alpha value must be between 0 and {MAX_ALPHA}.");
        Red = R;
        Green = G;
        Blue = B;
        Alpha = A;
    }

    /// <summary>
    /// Creates an <see cref="RgbColour"/> from an <see cref="HsvColour"/>.
    /// </summary>
    /// <param name="hsv">The <see cref="HsvColour"/> to convert to an <see cref="RgbColour"/>.</param>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as the provided <see cref="HsvColour"/>.</returns>
    public static RgbColour FromHsv(HsvColour hsv)
    {
        return hsv.ToRgb();
    }

    /// <summary>
    /// Creates an <see cref="RgbColour"/> from an <see cref="XyzColour"/>.
    /// </summary>
    /// <param name="xyz">The <see cref="XyzColour"/> to convert to an <see cref="RgbColour"/>.</param>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as the provided <see cref="XyzColour"/>.</returns>
    public static RgbColour FromXyz(XyzColour xyz)
    {
        return xyz.ToRgb();
    }

    /// <summary>
    /// Creates an <see cref="RgbColour"/> from a <see cref="LabColour"/>.
    /// </summary>
    /// <param name="lab">The <see cref="LabColour"/> to convert to an <see cref="RgbColour"/>.</param>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as the provided <see cref="LabColour"/>.</returns>
    public static RgbColour FromLab(LabColour lab)
    {
        return lab.ToXyz().ToRgb();
    }

    /// <summary>
    /// Creates an <see cref="RgbColour"/> from a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <param name="color">The <see cref="Microsoft.Xna.Framework.Color"/> to convert to an <see cref="RgbColour"/>.</param>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as the provided <see cref="Microsoft.Xna.Framework.Color"/>.</returns>
    public static RgbColour FromXnaColor(Color color)
    {
        return new RgbColour(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Creates an <see cref="RgbColour"/> from a hexadecimal colour string.
    /// </summary>
    /// <param name="hex">The hexadecimal colour string to convert to an <see cref="RgbColour"/>. It can be in the format "#RRGGBB" or "#RRGGBBAA", with or without a leading "#".</param>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as the provided hexadecimal string.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided hexadecimal string is null, whitespace, or not in a valid format.</exception>
    public static RgbColour FromHexString(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentException("Hex string cannot be null or whitespace.", nameof(hex));
        if (hex.StartsWith("#")) hex = hex[1..];
        if (hex.Length is not 6 and not 8) throw new ArgumentException("Hex string must be 6 or 8 characters long.", nameof(hex));

        decimal r = Convert.ToInt32(hex.Substring(0, 2), 16);
        decimal g = Convert.ToInt32(hex.Substring(2, 2), 16);
        decimal b = Convert.ToInt32(hex.Substring(4, 2), 16);
        decimal a = hex.Length == 8 ? Convert.ToInt32(hex.Substring(6, 2), 16) : MAX_ALPHA;

        return new RgbColour(r, g, b, a);
    }

    /// <summary>
    /// Converts this <see cref="RgbColour"/> to an <see cref="HsvColour"/>.
    /// </summary>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as this <see cref="RgbColour"/>.</returns>
    public HsvColour ToHsv()
    {
        decimal r = Red / MAX_RED;
        decimal g = Green / MAX_GREEN;
        decimal b = Blue / MAX_BLUE;
        decimal a = Alpha / MAX_ALPHA;

        decimal h, s;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);
        var delta = max - min;

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
            V: max * 100,
            A: a * 100
        );
    }

    public XyzColour ToXyz()
    {
        decimal r = Red / MAX_RED;
        decimal g = Green / MAX_GREEN;
        decimal b = Blue / MAX_BLUE;
        decimal a = Alpha / MAX_ALPHA;
        
        // See https://en.wikipedia.org/wiki/SRGB#Transfer_function_(%22gamma%22)
        r = r > 0.04045M ? (decimal)Math.Pow((double)((r + 0.055M) / 1.055M), 2.4) : r / 12.92M;
        g = g > 0.04045M ? (decimal)Math.Pow((double)((g + 0.055M) / 1.055M), 2.4) : g / 12.92M;
        b = b > 0.04045M ? (decimal)Math.Pow((double)((b + 0.055M) / 1.055M), 2.4) : b / 12.92M;

        // See http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html
        decimal x = r * 0.4124564M + g * 0.3575761M + b * 0.1804375M;
        decimal y = r * 0.2126729M + g * 0.7151522M + b * 0.0721750M;
        decimal z = r * 0.0193339M + g * 0.1191920M + b * 0.9503041M;

        return new XyzColour(
            X: x * 100,
            Y: y * 100,
            Z: z * 100,
            A: a * 100
        );
    }

    public LabColour ToLab()
    {
        return ToXyz().ToLab();
    }

    /// <summary>
    /// Converts this <see cref="RgbColour"/> to a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <returns>A <see cref="Microsoft.Xna.Framework.Color"/> representing the same colour as this <see cref="RgbColour"/>.</returns>
    public Color ToXnaColor()
    {
        return new Color(
            r: (int)Math.Round(Red),
            g: (int)Math.Round(Green),
            b: (int)Math.Round(Blue),
            alpha: (int)Math.Round(Alpha)
        );
    }

    /// <summary>
    /// Converts this <see cref="RgbColour"/> to a hexadecimal colour string.
    /// </summary>
    /// <param name="includeAlpha">If true, the resulting string will include the alpha component; otherwise, it will only include the red, green, and blue components.</param>
    /// <returns>A hexadecimal colour string representing the same colour as this <see cref="RgbColour"/> <b>without</b> a leading "#".</returns>
    public string ToHexString(bool includeAlpha = true)
    {
        return $"{(int)Math.Round(Red):X2}" +
               $"{(int)Math.Round(Green):X2}" +
               $"{(int)Math.Round(Blue):X2}" +
               $"{(includeAlpha ? $"{(int)Math.Round(Alpha):X2}" : "")}";
    }

    public static bool operator==(RgbColour lhs, RgbColour rhs)
    {
        return lhs.Red == rhs.Red && lhs.Green == rhs.Green && lhs.Blue == rhs.Blue && lhs.Alpha == rhs.Alpha;
    }
    
    public static bool operator!=(RgbColour lhs, RgbColour rhs)
    {
        return !(lhs == rhs);
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

    /// <summary>
    /// Returns a string representation of the <see cref="RgbColour"/> in the format:
    /// {R: <see cref="Red" />, G: <see cref="Green" />, B: <see cref="Blue" />, A: <see cref="Alpha" />}.
    /// </summary>
    /// <returns>A string representation of the <see cref="RgbColour"/>.</returns>
    public override string ToString()
    {
        return $"{{R: {Red}, G: {Green}, B: {Blue}, A: {Alpha}}}";
    }
}