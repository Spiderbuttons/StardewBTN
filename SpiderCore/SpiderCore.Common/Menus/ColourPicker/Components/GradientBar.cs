using System;
using SpiderCore.Common.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiderCore.Common.Shaders;
using StardewValley;

namespace SpiderCore.Common.Menus.ColourPicker.Components
{
    public class GradientBar
    {
        public static Effect GradientBarEffect
        {
            get
            {
                if (field is not null) return field;

                try
                {
                    field = ShaderUtilities.LoadShader("gradientBar");
                    return field;
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to load gradientBar shader: {e.Message}");
                    throw;
                }
            }
            set;
        }

        public Rectangle Bounds { get; set; }
        public bool IsHorizontal { get; set; } = true;
        public bool IsHueBar { get; set; }
        public bool IsAlphaBar { get; set; }
    
        public Color ColourOne { get; set; }
        public Color ColourTwo { get; set; }
    
        private static readonly BlendState SeeThroughBlendState = new()
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.Zero,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.Zero
        };
    
        public GradientBar(Rectangle? bounds = null, Color? colourOne = null, Color? colourTwo = null)
        {
            Bounds = bounds ?? Rectangle.Empty;
            ColourOne = colourOne ?? Color.Black;
            ColourTwo = colourTwo ?? Color.White;
        }

        public bool containsPoint(int x, int y)
        {
            return Bounds.Contains(new Point(x, y));
        }

        public void draw(SpriteBatch b, bool seeThrough = false)
        {
            if (Bounds.IsEmpty) return;
        
            b.End();
            b.Begin(blendState: seeThrough ? SeeThroughBlendState : null, effect: GradientBarEffect);

            GradientBarEffect.Parameters["IsHorizontal"].SetValue(IsHorizontal);
            GradientBarEffect.Parameters["ColourOne"].SetValue(ColourOne.ToVector4());
            GradientBarEffect.Parameters["ColourTwo"].SetValue(ColourTwo.ToVector4());
            GradientBarEffect.Parameters["Resolution"].SetValue(new Vector2(Bounds.Width, Bounds.Height));
            if (IsHueBar)
                GradientBarEffect.CurrentTechnique = GradientBarEffect.Techniques["HueBar"];
            else if (IsAlphaBar)
                GradientBarEffect.CurrentTechnique = GradientBarEffect.Techniques["AlphaBar"];
            else
            {
                GradientBarEffect.CurrentTechnique = GradientBarEffect.Techniques["GradientBar"];
            }

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
}