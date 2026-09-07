using System;
using System.IO;
using TheInfinityTones.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using TheInfinityTones.Helpers.ColourSpace;

namespace TheInfinityTones.Menus.ColourPickerMenu.Components;

public class ColourWheel : ClickableComponent
{
    public static Effect ColourWheelEffect
    {
        get
        {
            if (field is not null) return field;
            
            byte[] stream = File.ReadAllBytes(Path.Combine(ModEntry.ModHelper.DirectoryPath, "assets", "shaders", "colourWheel.mgfx"));
            field = new Effect(Game1.graphics.GraphicsDevice, stream);
            return field;
        }
        set;
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

    public bool Selected = false;

    public ColourWheel(string name, Vector2 centerPoint, float width, float height) : base(new Rectangle((int)centerPoint.X, (int)centerPoint.Y, (int)width, (int)height), name)
    {
        CenterPoint = centerPoint;
        Width = width;
        Height = height;
        Value = 1f;
    }

    public RgbColour GetColourAtScreenPoint(Vector2 point)
    {
        Vector2 unitSpacePoint = ScreenPointToUnitSpace(point);
        return GetColourAtUnitPoint(unitSpacePoint);
    }
    
    public static RgbColour GetColourAtUnitPoint(Vector2 point)
    {
        return PointToRgb(point);
    }
    
    public Rectangle GetBounds()
    {
        return new Rectangle(
            x: (int)(CenterPoint.X - Width / 2f),
            y: (int)(CenterPoint.Y - Height / 2f),
            width: (int)Width,
            height: (int)Height
        );
    }
    
    public Vector2 ClampPointToWheel(Vector2 point, int extraMargin = 0)
    {
        Vector2 direction = point - CenterPoint;
        float distance = direction.Length();
        float radius = Width / 2f;
        if (distance > radius + extraMargin)
        {
            direction.Normalize();
            direction *= radius + extraMargin;
            return CenterPoint + direction;
        }
        return point;
    }

    public Vector2 ScreenPointToUnitSpace(Vector2 point)
    {
        Vector2 direction = point - CenterPoint;
        direction /= Width;
        return direction;
    }

    public override bool containsPoint(int x, int y)
    {
        Vector2 point = new Vector2(x, y);
        Vector2 unitSpacePoint = ScreenPointToUnitSpace(point);
        return unitSpacePoint.Length() <= 0.5f;
    }

    public bool containsPointWithMargin(int x, int y, int extraMargin = 0)
    {
        Vector2 point = new Vector2(x, y);
        Vector2 unitSpacePoint = ScreenPointToUnitSpace(point);
        return unitSpacePoint.Length() <= 0.5f + extraMargin / Width;
    }

    public static RgbColour PointToRgb(Vector2 point)
    {
        HsvColour hsv = PointToHsv(point);
        return hsv.ToRgb();
    }
    
    public static HsvColour PointToHsv(Vector2 point)
    {
        /* https://www.shadertoy.com/view/3fcfWr */
        
        // This all assumes a -1 to 1 space.
        float radius = 1f;
        
        float distFromCenter = (point * 2).Length();
        float saturation = Math.Clamp(distFromCenter / radius, 0f, 1f);
        
        HsvColour finalColour = new HsvColour(
            H: (decimal)((Math.Atan2(point.Y, point.X) * 180 / Math.PI + 360) % 360),
            S: (decimal)(saturation * 100),
            V: 100M
        );
        return finalColour;
    }

    /// <summary>
    /// Convert an RGB colour to a point on the colour wheel.
    /// </summary>
    /// <param name="colour"></param>
    /// <returns>The point on the colour wheel that the RGB colour corresponds to, in unit space (-1 to 1).</returns>
    public static Vector2 RgbToPoint(RgbColour colour)
    {
        HsvColour hsv = colour.ToHsv();
        return HsvToPoint(hsv);
    }

    /// <summary>
    /// Convert an HSV colour to a point on the colour wheel.
    /// </summary>
    /// <param name="colour"></param>
    /// <returns>The point on the colour wheel that the HSV colour corresponds to, in unit space (-1 to 1).</returns>
    public static Vector2 HsvToPoint(HsvColour colour)
    {
        double angle = (double)colour.Hue * Math.PI / 180;
        double saturation = (double)(colour.Saturation / 100);
        return new Vector2((float)(saturation * Math.Cos(angle)), (float)(saturation * Math.Sin(angle)));
    }

    public void draw(SpriteBatch b, bool includeOutline = false)
    {
        b.End();
        b.Begin(effect: ColourWheelEffect);
        
        ColourWheelEffect.Parameters["Smoothing"].SetValue(Smoothing);
        ColourWheelEffect.Parameters["Resolution"].SetValue(new Vector2(Width * 5f, Height * 5f));
        
        ColourWheelEffect.Parameters["Value"].SetValue(0.1f);

        if (includeOutline)
        {
            b.Draw(
                texture: Game1.staminaRect,
                destinationRectangle: new Rectangle(
                    x: (int)CenterPoint.X,
                    y: (int)CenterPoint.Y,
                    width: (int)(Width * 1.1f),
                    height: (int)(Height * 1.1f)
                ),
                sourceRectangle: null,
                color: Color.Black * 0.75f,
                rotation: 0f,
                origin: new Vector2(0.5f, 0.5f),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }

        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(
                x: (int)CenterPoint.X,
                y: (int)CenterPoint.Y,
                width: (int)(Width * 1.005f),
                height: (int)(Height * 1.005f)
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