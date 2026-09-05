using System;
using Microsoft.Xna.Framework;
// ReSharper disable MemberCanBePrivate.Global

namespace TheInfinityTones.Helpers.ColourSpace;

/// <summary>
/// Represents a colour in the HSV (Hue, Saturation, Value) colour space, with hue, saturation, value, and alpha components.
/// </summary>
public readonly struct HsvColour : IEquatable<HsvColour>
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
    public readonly decimal Hue;
    
    /// <summary>
    /// The saturation component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Saturation;
    
    /// <summary>
    /// The value component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Value;

    /// <summary>
    /// The alpha (transparency) component of the colour, ranging from 0 to 100.
    /// </summary>
    public readonly decimal Alpha;
    
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