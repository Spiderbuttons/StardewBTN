using System;
using Microsoft.Xna.Framework;

namespace TheInfinityTones;

public struct SkinTone(Color darkest, Color medium, Color lightest) : IEquatable<SkinTone>
{
    public Color Darkest = darkest;
    public Color Medium = medium;
    public Color Lightest = lightest;

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