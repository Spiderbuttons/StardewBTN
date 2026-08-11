using System;
using System.IO;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FishPondDye.Components;

public class GradientBar
{
    private static Effect? _gradientBarEffect;
    public static Effect GradientBarEffect
    {
        get
        {
            if (_gradientBarEffect is not null) return _gradientBarEffect;

            try
            {
                byte[] stream = File.ReadAllBytes(Path.Combine(ModEntry.ModHelper.DirectoryPath, "assets", "shaders",
                    "gradientBar.mgfx"));
                _gradientBarEffect = new Effect(Game1.graphics.GraphicsDevice, stream);
                return _gradientBarEffect;
            } catch (Exception e)
            {
                Log.Error($"Failed to load gradientBar shader: {e.Message}");
                throw;
            }
        }
        set => _gradientBarEffect = value;
    }
    
    public Rectangle Bounds { get; set; }
    public bool IsHorizontal { get; set; }
    public bool IsHueBar  { get; set; }
    
    public Color ColourOne { get; set; }
    public Color ColourTwo { get; set; }
    
    public GradientBar(Rectangle? bounds = null, Color? colourOne = null, Color? colourTwo = null, bool isHorizontal = true, bool isHueBar = false)
    {
        Bounds = bounds ?? Rectangle.Empty;
        IsHorizontal = isHorizontal;
        IsHueBar = isHueBar;
        ColourOne = colourOne ?? Color.Black;
        ColourTwo = colourTwo ?? Color.White;
    }

    public bool ContainsPoint(Point point)
    {
        return Bounds.Contains(point);
    }

    public void draw(SpriteBatch b)
    {
        if (Bounds.IsEmpty) return;
        
        b.End();
        b.Begin(effect: GradientBarEffect);

        GradientBarEffect.Parameters["IsHorizontal"].SetValue(IsHorizontal);
        if (!IsHueBar)
        {
            GradientBarEffect.CurrentTechnique = GradientBarEffect.Techniques["GradientBar"];
            GradientBarEffect.Parameters["ColourOne"].SetValue(ColourOne.ToVector4());
            GradientBarEffect.Parameters["ColourTwo"].SetValue(ColourTwo.ToVector4());
        }
        else GradientBarEffect.CurrentTechnique = GradientBarEffect.Techniques["HueBar"];
        
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: Bounds,
            color: Color.White
        );
        
        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

        Color borderColour = new Color(104, 57, 36);
        int borderWidth = 2;
        
        // Left Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(Bounds.Left, Bounds.Y, borderWidth, Bounds.Height),
            color: borderColour * 0.7f
        );
        // Right Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(Bounds.Right - borderWidth, Bounds.Y, borderWidth, Bounds.Height),
            color: borderColour * 0.9f
        );
        // Top Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(Bounds.X + borderWidth, Bounds.Y, Bounds.Width - borderWidth * 2, borderWidth),
            color: borderColour * 0.7f
        );
        // Bottom Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(Bounds.X + borderWidth, Bounds.Bottom - borderWidth, Bounds.Width - borderWidth * 2, borderWidth),
            color: borderColour * 0.9f
        );
    }
}