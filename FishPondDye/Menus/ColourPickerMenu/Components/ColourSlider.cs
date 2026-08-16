using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu.Components;

public class ColourSlider : ClickableComponent
{
    private static readonly Rectangle CapSourceRect = new(435, 463, 6, 1);
    private static readonly Rectangle MiddleSourceRect = new(435, 464, 6, 8);

    private readonly decimal _min;
    private readonly decimal _max;
    
    private readonly Func<decimal> _getBackingValue;
    private readonly Action<decimal> _setBackingValue;
    public float Progress
    {
        get
        {
            float progress = (float)((_getBackingValue() - _min) / (_max - _min));
            return Math.Clamp(progress, 0f, 1f);
        }
        set
        {
            if (!IsHorizontal) value = 1f - value;
            _setBackingValue(Math.Clamp(_min + (decimal)value * (_max - _min), _min, _max));
        }
    }

    private float GrabberScale => IsHorizontal ? (float)Bar.Bounds.Height / MiddleSourceRect.Height : (float)Bar.Bounds.Width / MiddleSourceRect.Height;

    public bool Selected = false;

    public bool IsHorizontal
    {
        get => Bar.IsHorizontal;
        set => Bar.IsHorizontal = value;
    }
    
    public bool IsHueBar 
    {
        get => Bar.IsHueBar;
        set => Bar.IsHueBar = value;
    }
    
    public bool IsAlphaBar 
    {
        get => Bar.IsAlphaBar;
        set => Bar.IsAlphaBar = value;
    }
    
    public GradientBar Bar;
    
    public ColourSlider(string name, Func<decimal> getBackingValue, Action<decimal> setBackingValue, decimal min = 0, decimal max = 100, Rectangle? bounds = null, Color? colourOne = null, Color? colourTwo = null) : base(bounds ?? Rectangle.Empty, name)
    {
        _getBackingValue = getBackingValue;
        _setBackingValue = setBackingValue;
        _min = min;
        _max = max;
        Bar = new GradientBar(bounds, colourOne, colourTwo);
    }

    public void UpdateColours(Color one, Color two)
    {
        Bar.ColourOne = one;
        Bar.ColourTwo = two;
    }

    public void UpdateBarBounds(Rectangle bounds)
    {
        Bar.Bounds = bounds;
    }

    public Vector2 GetGrabberCenter()
    {
        if (Bar.Bounds.IsEmpty) return Vector2.Zero;

        if (IsHorizontal)
        {
            float x = Bar.Bounds.Left + Progress * Bar.Bounds.Width;
            float y = Bar.Bounds.Center.Y;
            return new Vector2(x, y);
        }
        else
        {
            float x = Bar.Bounds.Center.X;
            float y = Bar.Bounds.Bottom - Progress * Bar.Bounds.Height - (MiddleSourceRect.Width - CapSourceRect.Height) * GrabberScale / 2f;
            return new Vector2(x, y);
        }
    }
    
    public Rectangle GetGrabberBounds() 
    {
        Vector2 grabberCenter = GetGrabberCenter();
        if (IsHorizontal)
        {
            return new Rectangle(
                (int)(grabberCenter.X - MiddleSourceRect.Width * GrabberScale / 2f),
                (int)(grabberCenter.Y - MiddleSourceRect.Height * GrabberScale / 2f - CapSourceRect.Height * 2f),
                (int)(MiddleSourceRect.Width * GrabberScale),
                (int)(MiddleSourceRect.Height * GrabberScale + CapSourceRect.Height * 4f)
            );
        }
        else
        {
            return new Rectangle(
                (int)(grabberCenter.X - MiddleSourceRect.Height * GrabberScale / 2f - CapSourceRect.Height),
                (int)(grabberCenter.Y - MiddleSourceRect.Width * GrabberScale / 2f),
                (int)(MiddleSourceRect.Height * GrabberScale + CapSourceRect.Height * 2f),
                (int)(MiddleSourceRect.Width * GrabberScale)
            );
        }
    }

    public override bool containsPoint(int x, int y)
    {
        Point point = new(x, y);
        if (Bar.Bounds.Contains(point)) return true;
        
        Rectangle grabberBounds = GetGrabberBounds();
        return grabberBounds.Contains(point);
    }

    public void draw(SpriteBatch b)
    {
        if (Bar.Bounds.IsEmpty) return;
        
        Bar.draw(b);
        if (Progress >= 0) drawSliderGrabber(b);
    }
    
    public void drawSliderGrabber(SpriteBatch b)
    {
        Color grabberColour = Color.White;
        Vector2 center = GetGrabberCenter();

        if (IsHorizontal)
        {
            // Top Piece
            b.Draw(
                texture: Game1.mouseCursors,
                position: center - new Vector2(0, MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height * 2f),
                sourceRectangle: CapSourceRect,
                color: grabberColour,
                rotation: 0f,
                origin: new Vector2(CapSourceRect.Width / 2f, 0),
                scale: 2f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            // Middle Piece
            b.Draw(
                texture: Game1.mouseCursors,
                position: center,
                sourceRectangle: MiddleSourceRect,
                color: grabberColour,
                rotation: 0f,
                origin: new Vector2(MiddleSourceRect.Width / 2f, MiddleSourceRect.Height / 2f),
                scale: new Vector2(2f, GrabberScale),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            // Bottom Piece
            b.Draw(
                texture: Game1.mouseCursors,
                position: center + new Vector2(0, MiddleSourceRect.Height + CapSourceRect.Height * 2f),
                sourceRectangle: CapSourceRect,
                color: grabberColour,
                rotation: 0f,
                origin: new Vector2(CapSourceRect.Width / 2f, 0),
                scale: 2f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }
        else
        {
            // Left Piece
            b.Draw(
                texture: Game1.mouseCursors,
                position: center - new Vector2(MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height, 0),
                sourceRectangle: CapSourceRect,
                color: grabberColour,
                rotation: MathHelper.ToRadians(90f),
                origin: new Vector2(0, CapSourceRect.Height / 2f),
                scale: 2f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            // Middle Piece
            b.Draw(
                texture: Game1.mouseCursors,
                position: center,
                sourceRectangle: MiddleSourceRect,
                color: grabberColour,
                rotation: MathHelper.ToRadians(90f),
                origin: new Vector2(0, MiddleSourceRect.Height / 2f),
                scale: new Vector2(2f, GrabberScale),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            // Right Piece
            b.Draw(
                texture: Game1.mouseCursors,
                position: center + new Vector2(MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height, 0),
                sourceRectangle: CapSourceRect,
                color: grabberColour,
                rotation: MathHelper.ToRadians(90f),
                origin: new Vector2(0, CapSourceRect.Height / 2f),
                scale: 2f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }
    }
}