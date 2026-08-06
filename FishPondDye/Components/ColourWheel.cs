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
    public static Effect ColourWheelEffect
    {
        get => field ??= LoadColourWheelFx() ?? throw new Exception("Failed to load ColourWheel shader.");
        set;
    }

    private static Effect? LoadColourWheelFx()
    {
        try
        {
            byte[] stream = File.ReadAllBytes(Path.Combine(ModEntry.ModHelper.DirectoryPath, "assets", "shaders", "colourWheel.mgfx"));
            return new Effect(Game1.graphics.GraphicsDevice, stream);
        } catch (Exception e)
        {
            Log.Error(e);
            return null;
        }
    }

    public int Width { get; set; }
    public int Height { get; set; }

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

    public ColourWheel(int width, int height)
    {
        Width = width;
        Height = height;
        Value = 1f;
    }
    
    public static Texture2D GenerateColourWheelTexture(int width, int height)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Texture2D wheel = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // The math was easier for me if it was from -1 to 1 so...
                float normalizedX = x / (float)(width - 1) * 2f - 1f;
                float normalizedY = y / (float)(height - 1) * 2f - 1f;

                Vector2 point = new Vector2(normalizedX, normalizedY);
                Color colour = PointToRGB(point);
                pixels[y * width + x] = colour;
            }
        }
        
        // Without any blurring it still looks pixelated even at high resolutions so it's time for some ~CONVolutions~
        // This is just implementing an accumulator box blur. I could probably do it in the initial loop up above but uhh idk how.
        int blurSize = 3;
        float avg = 1f / blurSize;
        
        // Horizontal Blur
        for (int y = 0; y < height; y++)
        {
            float[] hSum = [0f, 0f, 0f, 0f];
            float[] iAvg = [0f, 0f, 0f, 0f];
            for (int x = 0; x < blurSize; x++)
            {
                Color temp = pixels[y * width + x];
                hSum[0] += temp.R;
                hSum[1] += temp.G;
                hSum[2] += temp.B;
                hSum[3] += temp.A;
            }

            iAvg[0] = hSum[0] * avg;
            iAvg[1] = hSum[1] * avg;
            iAvg[2] = hSum[2] * avg;
            iAvg[3] = hSum[3] * avg;
            for (int x = 0; x < width; x++)
            {
                if (x - blurSize / 2 >= 0 && x + 1 + blurSize / 2 < width)
                {
                    Color tempP = pixels[x - blurSize / 2 + y * width];
                    Color tempN = pixels[x + 1 + blurSize / 2 + y * width];
                    hSum[0] += tempN.R - tempP.R;
                    hSum[1] += tempN.G - tempP.G;
                    hSum[2] += tempN.B - tempP.B;
                    hSum[3] += tempN.A - tempP.A;
                    iAvg[0] = hSum[0] * avg;
                    iAvg[1] = hSum[1] * avg;
                    iAvg[2] = hSum[2] * avg;
                    iAvg[3] = hSum[3] * avg;
                }
                
                pixels[x + y * width] = new Color((byte)iAvg[0], (byte)iAvg[1], (byte)iAvg[2], (byte)iAvg[3]);
            }
        }
        
        // Vertical Blur
        for (int x = 0; x < width; x++)
        {
            float[] vSum = [0f, 0f, 0f, 0f];
            float[] iAvg = [0f, 0f, 0f, 0f];
            for (int y = 0; y < blurSize; y++)
            {
                Color temp = pixels[x + y * width];
                vSum[0] += temp.R;
                vSum[1] += temp.G;
                vSum[2] += temp.B;
                vSum[3] += temp.A;
            }
            
            iAvg[0] = vSum[0] * avg;
            iAvg[1] = vSum[1] * avg;
            iAvg[2] = vSum[2] * avg;
            iAvg[3] = vSum[3] * avg;
            for (int y = 0; y < height; y++)
            {
                if (y == height - 1) break; // Looks kinda ugly on the bottom row for some reason.
                if (y - blurSize / 2 >= 0 && y + 1 + blurSize / 2 < height)
                {
                    Color tempP = pixels[x + (y - blurSize / 2) * width];
                    Color tempN = pixels[x + (y + 1 + blurSize / 2) * width];
                    vSum[0] += tempN.R - tempP.R;
                    vSum[1] += tempN.G - tempP.G;
                    vSum[2] += tempN.B - tempP.B;
                    vSum[3] += tempN.A - tempP.A;
                    iAvg[0] = vSum[0] * avg;
                    iAvg[1] = vSum[1] * avg;
                    iAvg[2] = vSum[2] * avg;
                    iAvg[3] = vSum[3] * avg;
                }
                
                pixels[x + y * width] = new Color((byte)iAvg[0], (byte)iAvg[1], (byte)iAvg[2], (byte)iAvg[3]);
            }
        }

        wheel.SetData(pixels);
        stopwatch.Stop();
        Log.Debug($"Colour wheel generated in {stopwatch.ElapsedMilliseconds} ms");
        return wheel;
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
        float distFromCenter = point.Length();
        float saturation = Math.Clamp(distFromCenter / radius, 0f, 1f);
        
        Vector3 rgb = Vector3.Lerp(hueRGB, smoothHueRGB, 1f);
        Vector3 result = Vector3.Lerp(Vector3.One, rgb, saturation);

        if (distFromCenter > radius) return Color.Transparent;
        Color finalColour = new Color(result.X, result.Y, result.Z);
        return finalColour;
        
        // The below code is for the more exactly correct colour wheel, as opposed to the above which makes a "smoothed" version.
        // float radius = point.Length();
        // if (radius > 1f) return Color.Transparent; // Outside the wheel
        //
        // double angle = Math.Atan2(point.Y, point.X);
        // if (angle < 0) angle += MathHelper.TwoPi;
        // double hue = angle / MathHelper.TwoPi;
        // double saturation = radius;
        // const double value = 1f;
        //     
        // return ColorPicker.HsvToRgb(hue * 360f, saturation, value);
    }

    public void draw(SpriteBatch b, Vector2 position, float scale)
    {
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, effect: ColourWheelEffect);

        ColourWheelEffect.Parameters["Resolution"].SetValue(new Vector2(Width, Height));
        ColourWheelEffect.Parameters["Value"].SetValue(1.0f);
        ColourWheelEffect.Parameters["Smoothing"].SetValue(1.0f);
        
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(
                (int)(position.X - Width * scale),
                (int)(position.Y - Height * scale),
                (int)(Width * scale),
                (int)(Height * scale)
            ),
            color: Color.White
        );

        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
    }
}