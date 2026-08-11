using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FishPondDye.Components;

public class ColourWheel
{
    private static Effect? _colourWheelEffect;
    public static Effect ColourWheelEffect
    {
        get
        {
            if (_colourWheelEffect is not null) return _colourWheelEffect;
            
            byte[] stream = File.ReadAllBytes(Path.Combine(ModEntry.ModHelper.DirectoryPath, "assets", "shaders", "colourWheel.mgfx"));
            _colourWheelEffect = new Effect(Game1.graphics.GraphicsDevice, stream);
            return _colourWheelEffect;
        }
        set => _colourWheelEffect = value;
    }
    
    public Vector2 CenterPoint { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public float Value
    {
        get;
        set => field = Math.Clamp(value, 0f, 1f);
    }

    public float Smoothing
    {
        get;
        set => field = Math.Clamp(value, 0f, 1f);
    }

    public ColourWheel(Vector2 centerPoint, float width, float height)
    {
        CenterPoint = centerPoint;
        Width = width;
        Height = height;
        Value = 1f;
    }

    public Color GetColourAtPoint(Vector2 point)
    {
        Vector2 distance = point - CenterPoint;
        distance /= new Vector2(Width, Height);
        
        Color rgb = PointToRGB(distance);
        return rgb;
    }

    public Color GetColourAtCursor()
    {
        Point cursorPos = Game1.getMousePosition();
        return GetColourAtPoint(cursorPos.ToVector2());
    }

    public bool Contains(Vector2 point)
    {
        Vector2 distance = point - CenterPoint;
        float radius = Width / 2f;
        return distance.Length() <= radius;
    }

    public static Vector3 RGBToHSV(Color color)
    {
        var (r, g, b) = (color.R / 255d, color.G / 255d, color.B / 255d);
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;
        
        double hue;
        const float TOLERANCE = 0.0000000001f;
        if (delta == 0)
            hue = 0;
        else if (Math.Abs(max - r) < TOLERANCE)
            hue = (60 * ((g - b) / delta) + 360) % 360;
        else if (Math.Abs(max - g) < TOLERANCE)
            hue = (60 * ((b - r) / delta) + 120) % 360;
        else
            hue = (60 * ((r - g) / delta) + 240) % 360;
        
        double saturation = max == 0 ? 0 : delta / max;
        double value = max;

        return new Vector3((float)hue, (float)saturation, (float)value);
    }

    public static Color PointToRGB(Vector2 point)
    {
        /* https://www.shadertoy.com/view/3fcfWr */
        
        // This all assumes a -1 to 1 space.
        float radius = 1f;
        float hue = (float)(Math.Atan2(point.Y, point.X) * 3f / Math.PI);
        Vector3 hueRGB = new Vector3(hue, hue - 2f, hue + 2f);
        hueRGB = new Vector3(
            Math.Clamp(Math.Abs(3f - Math.Abs(hueRGB.X)) - 1, 0f, 1f),
            Math.Clamp(Math.Abs(3f - Math.Abs(hueRGB.Y)) - 1, 0f, 1f),
            Math.Clamp(Math.Abs(3f - Math.Abs(hueRGB.Z)) - 1, 0f, 1f)
        );

        Vector3 smoothHueRGB = hueRGB * hueRGB * (new Vector3(3) - hueRGB * 2f);
        float distFromCenter = (point * 2).Length();
        float saturation = Math.Clamp(distFromCenter / radius, 0f, 1f);
        
        Vector3 rgb = Vector3.Lerp(hueRGB, smoothHueRGB, 0f);
        Vector3 result = Vector3.Lerp(Vector3.One, rgb, saturation);
        
        Color finalColour = new Color(result.X, result.Y, result.Z);
        return finalColour;
    }

    public static Vector2 RGBToPoint(Color color)
    {
        var (r, g, b) = (color.R / 255d, color.G / 255d, color.B / 255d);
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;
        
        double hue;
        const float TOLERANCE = 0.0000000001f;
        if (delta == 0)
            hue = 0;
        else if (Math.Abs(max - r) < TOLERANCE)
            hue = (60 * ((g - b) / delta) + 360) % 360;
        else if (Math.Abs(max - g) < TOLERANCE)
            hue = (60 * ((b - r) / delta) + 120) % 360;
        else
            hue = (60 * ((r - g) / delta) + 240) % 360;
        
        double saturation = max == 0 ? 0 : delta / max;
        
        double angle = hue * Math.PI / 180;
        return new Vector2((float)(saturation * Math.Cos(angle)), (float)(saturation * Math.Sin(angle)));
    }

    public void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(effect: ColourWheelEffect);
        
        ColourWheelEffect.Parameters["Resolution"].SetValue(new Vector2(Width * 5f, Height * 5f));
        
        ColourWheelEffect.Parameters["Value"].SetValue(0.1f);
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(
                x: (int)CenterPoint.X,
                y: (int)CenterPoint.Y,
                width: (int)(Width * 1.01f),
                height: (int)(Height * 1.01f)
            ),
            sourceRectangle: null,
            color: new Color(84, 35, 15) * 0.4f,
            rotation: 0f,
            origin: new Vector2(0.5f, 0.5f),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        ColourWheelEffect.Parameters["Value"].SetValue(Value);
        
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(
                x: (int)CenterPoint.X,
                y: (int)CenterPoint.Y,
                width: (int)Width,
                height: (int)Height
            ),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: new Vector2(0.5f, 0.5f),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );

        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
    }
}