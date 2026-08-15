using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FishPondDye.Menus.ColourPickerMenu.Components;

public class ColourSlider
{
    private static Rectangle CapSourceRect = new(435, 463, 6, 1);
    private static Rectangle MiddleSourceRect = new(435, 464, 6, 8);
    
    private readonly Func<decimal> _getBackingValue;
    public float Progress => (float)_getBackingValue();
    public bool IsSelected = false;

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
    
    public ColourSlider(Func<decimal> getter, Rectangle? bounds = null, Color? colourOne = null, Color? colourTwo = null)
    {
        _getBackingValue = getter;
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

    public Rectangle GetGrabberBounds()
    {
        Vector2 grabberPosition = new Vector2(Bar.Bounds.X + Bar.Bounds.Width * Progress, Bar.Bounds.Top - CapSourceRect.Height);
        int grabberHeight = Bar.Bounds.Height + CapSourceRect.Height * 2;
        return new Rectangle(
            x: (int)(grabberPosition.X - MiddleSourceRect.Width / 2f),
            y: (int)(grabberPosition.Y),
            width: MiddleSourceRect.Width,
            height: grabberHeight
        );
    }

    public bool ContainsPoint(Point point)
    {
        return Bar.ContainsPoint(point) || GetGrabberBounds().Contains(point);
    }

    public void draw(SpriteBatch b)
    {
        if (Bar.Bounds.IsEmpty) return;
        
        Bar.draw(b);
        if (Progress >= 0) drawSliderGrabber(b);
    }
    
    public void drawSliderGrabber(SpriteBatch b)
    {
        Rectangle grabberBounds = GetGrabberBounds();
        float middleScale = (float)Bar.Bounds.Height / MiddleSourceRect.Height;
        b.Draw(
            texture: Game1.mouseCursors,
            position: new Vector2(grabberBounds.X + CapSourceRect.Width / 2f, grabberBounds.Top),
            sourceRectangle: CapSourceRect,
            color: Color.White,
            rotation: 0f,
            origin: new Vector2(CapSourceRect.Width / 2f, CapSourceRect.Height / 2f),
            scale: 2f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        b.Draw(
            texture: Game1.mouseCursors,
            position: new Vector2(grabberBounds.X + MiddleSourceRect.Width / 2f, grabberBounds.Top + CapSourceRect.Height),
            sourceRectangle: MiddleSourceRect,
            color: Color.White,
            rotation: 0f,
            origin: new Vector2(MiddleSourceRect.Width / 2f, 0),
            scale: new Vector2(2f, middleScale),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        b.Draw(
            texture: Game1.mouseCursors,
            position: new Vector2(grabberBounds.X + CapSourceRect.Width / 2f, grabberBounds.Bottom),
            sourceRectangle: CapSourceRect,
            color: Color.White,
            rotation: MathHelper.ToRadians(180f),
            origin: new Vector2(CapSourceRect.Width / 2f, CapSourceRect.Height / 2f),
            scale: 2f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
    }
}