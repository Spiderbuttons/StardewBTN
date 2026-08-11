using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FishPondDye.Components;

public class ColourSlider
{
    private static Rectangle CapSourceRect = new(435, 463, 6, 1);
    private static Rectangle MiddleSourceRect = new(435, 464, 6, 8);
    
    public GradientBar Bar;
    public float BarProgress = 1f;
    
    public ColourSlider(Rectangle? bounds = null, Color? colourOne = null, Color? colourTwo = null, bool isHorizontal = true, bool isHueBar = false)
    {
        Bar = new GradientBar(bounds, colourOne, colourTwo, isHorizontal, isHueBar);
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
    
    public void UpdateProgress(float progress)
    {
        BarProgress = Math.Clamp(progress, 0f, 1f);
    }

    public Rectangle GetGrabberBounds()
    {
        Vector2 grabberPosition = new Vector2(Bar.Bounds.X + Bar.Bounds.Width * BarProgress, Bar.Bounds.Top - CapSourceRect.Height);
        int grabberHeight = Bar.Bounds.Height + CapSourceRect.Height * 2;
        return new Rectangle(
            x: (int)(grabberPosition.X - MiddleSourceRect.Width / 2f),
            y: (int)(grabberPosition.Y),
            width: MiddleSourceRect.Width,
            height: grabberHeight
        );
    }

    public void draw(SpriteBatch b)
    {
        if (Bar.Bounds.IsEmpty) return;
        
        Bar.draw(b);
        drawSliderGrabber(b);
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

        // b.Draw(
        //     texture: _sliderGrabberCap,
        //     position: new Vector2(grabberBounds.X + _sliderGrabberCap.Width / 2f, grabberBounds.Top + _sliderGrabberCap.Height),
        //     sourceRectangle: null,
        //     color: Color.White,
        //     rotation: 0f,
        //     scale: 2f,
        //     origin: new Vector2(2.5f, _sliderGrabberCap.Height / 2f),
        //     effects: SpriteEffects.None,
        //     layerDepth: 1f
        // );
        // b.Draw(
        //     texture: _sliderGrabberMiddle,
        //     position: new Vector2(grabberBounds.X + _sliderGrabberMiddle.Width / 2f, grabberBounds.Center.Y),
        //     sourceRectangle: null,
        //     color: Color.White,
        //     rotation: 0f,
        //     scale: new Vector2(2f, Bar.Bounds.Height),
        //     origin: new Vector2(2.5f, _sliderGrabberMiddle.Height / 2f),
        //     effects: SpriteEffects.None,
        //     layerDepth: 1f
        // );
        // b.Draw(
        //     texture: _sliderGrabberCap,
        //     position: new Vector2(grabberBounds.X + _sliderGrabberCap.Width / 2f, grabberBounds.Bottom - _sliderGrabberCap.Height),
        //     sourceRectangle: null,
        //     color: Color.White,
        //     rotation: MathHelper.ToRadians(180f),
        //     scale: 2f,
        //     origin: new Vector2(2.5f, _sliderGrabberCap.Height / 2f),
        //     effects: SpriteEffects.None,
        //     layerDepth: 1f
        // );
    }
}