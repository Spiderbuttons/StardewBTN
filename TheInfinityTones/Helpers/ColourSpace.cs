using System;
using Microsoft.Xna.Framework;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace TheInfinityTones.Helpers;

/// <summary>
/// Represents a colour in the RGB colour space, with red, green, blue, and alpha components.
/// </summary>
public struct RgbColour : IEquatable<RgbColour>
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
        decimal min, max, delta;

        decimal r = Red / MAX_RED;
        decimal g = Green / MAX_GREEN;
        decimal b = Blue / MAX_BLUE;
        decimal a = Alpha / MAX_ALPHA;

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

/// <summary>
/// Represents a colour in the HSV (Hue, Saturation, Value) colour space, with hue, saturation, value, and alpha components.
/// </summary>
public struct HsvColour : IEquatable<HsvColour>
{
    /// <summary>The maximum possible value for the hue component of the colour.</summary>
    public const decimal MAX_HUE = 360M;
    /// <summary>The maximum possible value for the saturation component of the colour.</summary>
    public const decimal MAX_SATURATION = 100M;
    /// <summary>The maximum possible value for the value component of the colour.</summary>
    public const decimal MAX_VALUE = 100M;
    /// <summary>The maximum possible value for the alpha (transparency) component of the colour.</summary>
    public const decimal MAX_ALPHA = 100M;
    
    /// <summary>
    /// The hue component of the colour, ranging from 0 to 360.
    /// </summary>
    public decimal Hue;
    
    /// <summary>
    /// The saturation component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Saturation;
    
    /// <summary>
    /// The value component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Value;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Alpha;
    
    /// <inheritdoc cref="Hue" />
    public decimal H => Hue;
    
    /// <inheritdoc cref="Saturation" />
    public decimal S => Saturation;
    
    /// <inheritdoc cref="Value" />
    public decimal V => Value;
    
    /// <inheritdoc cref="Alpha" />
    public decimal A => Alpha;

    /// <summary>
    /// Initializes a new instance of an <see cref="HsvColour"/> struct with the specified <paramref name="H"/>, <paramref name="S"/>, <paramref name="V"/>, and optional <paramref name="A"/> values.
    /// </summary>
    /// <param name="H">The hue component of the colour, ranging from 0 to 360.</param>
    /// <param name="S">The saturation component of the colour, ranging from 0 to 100.</param>
    /// <param name="V">The value component of the colour, ranging from 0 to 100.</param>
    /// <param name="A">The alpha (transparency) component of the colour, ranging from 0 to 100. Defaults to 100 (fully opaque).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the colour components are outside their valid ranges.</exception>
    public HsvColour(decimal H, decimal S, decimal V, decimal A = MAX_ALPHA)
    {
        if (H is < 0 or > MAX_HUE) throw new ArgumentOutOfRangeException(nameof(H), $"Hue value must be between 0 and {MAX_HUE}.");
        if (S is < 0 or > MAX_SATURATION) throw new ArgumentOutOfRangeException(nameof(S), $"Saturation value must be between 0 and {MAX_SATURATION}.");
        if (V is < 0 or > MAX_VALUE) throw new ArgumentOutOfRangeException(nameof(V), $"Value must be between 0 and {MAX_VALUE}.");
        if (A is < 0 or > MAX_ALPHA) throw new ArgumentOutOfRangeException(nameof(A), $"Alpha must be between 0 and {MAX_ALPHA}.");
        Hue = H;
        Saturation = S;
        Value = V;
        Alpha = A;
    }

    /// <summary>
    /// Creates an <see cref="HsvColour"/> from an <see cref="RgbColour"/>.
    /// </summary>
    /// <param name="rgb">The <see cref="RgbColour"/> to convert to an <see cref="HsvColour"/>.</param>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as the provided <see cref="RgbColour"/>.</returns>
    public static HsvColour FromRgb(RgbColour rgb)
    {
        return rgb.ToHsv();
    }

    /// <summary>
    /// Creates an <see cref="HsvColour"/> from an <see cref="XyzColour"/>.
    /// </summary>
    /// <param name="xyz">The <see cref="XyzColour"/> to convert to an <see cref="HsvColour"/>.</param>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as the provided <see cref="XyzColour"/>.</returns>
    public static HsvColour FromXyz(XyzColour xyz)
    {
        return RgbColour.FromXyz(xyz).ToHsv();
    }

    /// <summary>
    /// Creates an <see cref="HsvColour"/> from a <see cref="LabColour"/>.
    /// </summary>
    /// <param name="lab">The <see cref="LabColour"/> to convert to an <see cref="HsvColour"/>.</param>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as the provided <see cref="LabColour"/>.</returns>
    public static HsvColour FromLab(LabColour lab)
    {
        return RgbColour.FromLab(lab).ToHsv();
    }

    /// <summary>
    /// Creates an <see cref="HsvColour"/> from a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <param name="color">The <see cref="Microsoft.Xna.Framework.Color"/> to convert to an <see cref="HsvColour"/>.</param>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as the provided <see cref="Microsoft.Xna.Framework.Color"/>.</returns>
    public static HsvColour FromXnaColor(Color color)
    {
        return RgbColour.FromXnaColor(color).ToHsv();
    }

    /// <summary>
    /// Creates an <see cref="HsvColour"/> from a hexadecimal colour string.
    /// </summary>
    /// <param name="hex">The hexadecimal colour string to convert to an <see cref="HsvColour"/>. It can be in the format "#RRGGBB" or "#RRGGBBAA", with or without a leading "#".</param>
    /// <returns>An <see cref="HsvColour"/> representing the same colour as the provided hexadecimal string.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided hexadecimal string is null, whitespace, or not in a valid format.</exception>
    public static HsvColour FromHexString(string hex)
    {
        return RgbColour.FromHexString(hex).ToHsv();
    }

    /// <summary>
    /// Converts this <see cref="HsvColour"/> to an <see cref="RgbColour"/>.
    /// </summary>
    /// <returns>An <see cref="RgbColour"/> representing the same colour as this <see cref="HsvColour"/>.</returns>
    public RgbColour ToRgb()
    {
        decimal r, g, b;

        decimal h = Hue % MAX_HUE;
        decimal s = Saturation / MAX_SATURATION;
        decimal v = Value / MAX_VALUE;
        decimal a = Alpha / MAX_ALPHA;

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
            R: r * RgbColour.MAX_RED,
            G: g * RgbColour.MAX_GREEN,
            B: b * RgbColour.MAX_BLUE,
            A: a * RgbColour.MAX_ALPHA
        );
    }

    /// <summary>
    /// Converts this <see cref="HsvColour"/> to an <see cref="XyzColour"/>.
    /// </summary>
    /// <returns>An <see cref="XyzColour"/> representing the same colour as this <see cref="HsvColour"/>.</returns>
    public XyzColour ToXyz()
    {
        return ToRgb().ToXyz();
    }

    /// <summary>
    /// Converts this <see cref="HsvColour"/> to a <see cref="LabColour"/>.
    /// </summary>
    /// <returns>A <see cref="LabColour"/> representing the same colour as this <see cref="HsvColour"/>.</returns>
    public LabColour ToLab()
    {
        return ToXyz().ToLab();
    }

    /// <summary>
    /// Converts this <see cref="HsvColour"/> to a <see cref="Microsoft.Xna.Framework.Color"/>.
    /// </summary>
    /// <returns>A <see cref="Microsoft.Xna.Framework.Color"/> representing the same colour as this <see cref="HsvColour"/>.</returns>
    public Color ToXnaColor()
    {
        return ToRgb().ToXnaColor();
    }

    /// <summary>
    /// Converts this <see cref="HsvColour"/> to a hexadecimal colour string.
    /// </summary>
    /// <param name="includeAlpha">If true, the resulting string will include the alpha component; otherwise, it will only include the red, green, and blue components.</param>
    /// <returns>A hexadecimal colour string representing the same colour as this <see cref="HsvColour"/> <b>without</b> a leading "#".</returns>
    public string ToHexString(bool includeAlpha = true)
    {
        return ToRgb().ToHexString(includeAlpha);
    }

    public static bool operator==(HsvColour lhs, HsvColour rhs)
    {
        return lhs.Hue == rhs.Hue && lhs.Saturation == rhs.Saturation && lhs.Value == rhs.Value && lhs.Alpha == rhs.Alpha;
    }
    
    public static bool operator!=(HsvColour lhs, HsvColour rhs)
    {
        return !(lhs == rhs);
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
    
    /// <summary>
    /// Returns a string representation of the <see cref="HsvColour"/> in the format:
    /// {H: <see cref="Hue" />, S: <see cref="Saturation" />, V: <see cref="Value" />, A: <see cref="Alpha" />}.
    /// </summary>
    /// <returns>A string representation of the <see cref="HsvColour"/>.</returns>
    public override string ToString()
    {
        return $"{{H: {Hue}, S: {Saturation}, V: {Value}, A: {Alpha}}}";
    }
}

/// <summary>
/// Represents a colour in the XYZ colour space, with X, Y, Z, and alpha components relative to <see href="https://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D">CIE Standard illuminant D65</see>.
/// </summary>
public struct XyzColour : IEquatable<XyzColour>
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
    public decimal X;

    /// <summary>
    /// The Y component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Y;

    /// <summary>
    /// The Z component of the colour, ranging from 0 to 108.884.
    /// </summary>
    public decimal Z;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Alpha;

    /// <inheritdoc cref="A" />
    public decimal A;

    /// <summary>
    /// Initializes a new instance of an <see cref="XyzColour"/> struct with the specified <paramref name="X"/>, <paramref name="Y"/>, <paramref name="Z"/>, and optional <paramref name="A"/> values.
    /// </summary>
    /// <param name="X">The X component of the colour, ranging from 0 to 95.0489.</param>
    /// <param name="Y">The Y component of the colour, ranging from 0 to 100.</param>
    /// <param name="Z">The Z component of the colour, ranging from 0 to 108.884.</param>
    /// <param name="A">The alpha (transparency) component of the colour, ranging from 0 to 100. Defaults to 100 (fully opaque).</param>
    /// <remarks>The X, Y, and Z components will be clamped if out of range rather than throw an exception, as the exact ranges may differ depending on the reference illuminant.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the alpha component is outside its valid range.</exception>
    public XyzColour(decimal X, decimal Y, decimal Z, decimal A = MAX_ALPHA)
    {
        this.X = Math.Clamp(X, 0, MAX_X);
        this.Y = Math.Clamp(Y, 0, MAX_Y);
        this.Z = Math.Clamp(Z, 0, MAX_Z);
        if (A is < 0 or > MAX_ALPHA) throw new ArgumentOutOfRangeException(nameof(A), $"Alpha value must be between 0 and {MAX_ALPHA}.");
        Alpha = A;
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
        return lab.ToXyz();
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

    public LabColour ToLab()
    {
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

public struct LabColour : IEquatable<LabColour>
{
    /// <summary>The maximum possible value for the L component of the colour.</summary>
    public const decimal MAX_L = 100M;
    /// <summary>The maximum possible value for the alpha (transparency) component of the colour.</summary>
    public const decimal MAX_ALPHA = 100M;
    
    /// <summary>
    /// The L component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal L;
    
    /// <summary>
    /// The A component of the colour.
    /// </summary>
    public decimal A;
    
    /// <summary>
    /// The B component of the colour.
    /// </summary>
    public decimal B;
    
    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public decimal Alpha;
    
    /// <summary>
    /// Initializes a new instance of an <see cref="LabColour"/> struct with the specified <paramref name="L"/>, <paramref name="A"/>, <paramref name="B"/>, and optional <paramref name="Alpha"/> values.
    /// </summary>
    /// <param name="L">The L component of the colour, ranging from 0 to 100.</param>
    /// <param name="A">The A component of the colour.</param>
    /// <param name="B">The B component of the colour.</param>
    /// <param name="Alpha">The alpha (transparency) component of the colour, ranging from 0 to 100. Defaults to 100 (fully opaque).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the L or Alpha component is outside its valid range.</exception>
    public LabColour(decimal L, decimal A, decimal B, decimal Alpha = MAX_ALPHA)
    {
        if (L is < 0 or > MAX_L) throw new ArgumentOutOfRangeException(nameof(L), $"L value must be between 0 and {MAX_L}.");
        this.L = L;
        this.A = A;
        this.B = B;
        if (Alpha is < 0 or > MAX_ALPHA) throw new ArgumentOutOfRangeException(nameof(Alpha), $"Alpha value must be between 0 and {MAX_ALPHA}.");
        this.Alpha = Alpha;
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
            A: Alpha
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