using System;
using FishPondDye.Components;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FishPondDye;

public class ColourPicker : IClickableMenu
{
    private static Texture2D _selectionCircle = GenerateSelectionOutline();
    
    private FishPond? _pond = null;

    private readonly ColourWheel _colourWheel = new(
        new Vector2(
            x: Game1.uiViewport.Width / 2f,
            y: Game1.uiViewport.Height / 3f), 
        width: (int)(Game1.uiViewport.Height / 4f), 
        height: (int)(Game1.uiViewport.Height / 4f));
    
    private ColourSlider RedSlider = new();
    private ColourSlider BlueSlider = new();
    private ColourSlider GreenSlider = new();
    private ColourSlider HueSlider = new(isHueBar: true);
    private ColourSlider SaturationSlider = new();
    private ColourSlider ValueSlider = new();
    
    private float _value = 1f;
    
    public Color SelectedColour = Color.White;

    private bool _isSelecting = false;

    public ColourPicker(FishPond? pond)
    {
        _pond = pond;
    }
    
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        _isSelecting = false;
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        if (_isSelecting) SelectedColour = _colourWheel.GetColourAtPoint(new Vector2(x, y));
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        if (_isSelecting) return;
        if (_colourWheel.Contains(new Vector2(x,y)))
        {
            _isSelecting = true;
            SelectedColour = _colourWheel.GetColourAtCursor();
            return;
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        base.receiveKeyPress(key);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
    }

    private static Texture2D GenerateSelectionOutline()
    {
        Texture2D outline = new Texture2D(Game1.graphics.GraphicsDevice, 9, 9, mipmap: false, format: SurfaceFormat.Color);
        byte[] shape =
        [
            0,0,1,1,1,1,1,0,0,
            0,1,3,3,3,3,3,1,0,
            1,3,0,0,0,0,0,3,1,
            1,4,0,0,0,0,0,4,1,
            2,4,0,0,0,0,0,4,2,
            2,4,0,0,0,0,0,4,2,
            2,5,0,0,0,0,0,5,2,
            0,2,6,5,5,5,6,2,0,
            0,0,2,2,2,2,2,0,0
        ];
        Color[] data = new Color[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            data[i] = shape[i] switch 
            {
                1 => new Color(88, 29, 43),
                2 => new Color(29, 17, 9),
                3 => new Color(251, 249, 0) * 0.8f,
                4 => new Color(242, 188, 82) * 0.8f,
                5 => new Color(220, 123, 5) * 0.8f,
                6 => new Color(177, 78, 5) * 0.8f,
                _ => Color.Transparent
            };
        }
        outline.SetData(data);
        return outline;
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.4f);
        
        // draw a box in the center of the screen that is 1/3rd the screen width and half the screen height
        Game1.DrawBox(
            x: Game1.uiViewport.Width / 2 - Game1.uiViewport.Width / 6,
            y: Game1.uiViewport.Height / 2 - Game1.uiViewport.Width / 6,
            width: Game1.uiViewport.Width / 3,
            height: Game1.uiViewport.Width / 3
        );
        
        _colourWheel.draw(b);
        drawSelectionCircle(b);
        drawRgbBars(b);
        drawSliderGrabbers(b);

        // b.Draw(Game1.staminaRect, new Rectangle(rectX, rectY, rectWidth, rectHeight), color: SelectedColour);
        drawMouse(b);
    }

    public void drawRgbBars(SpriteBatch b)
    {
        Vector2 rgbStrings = Game1.smallFont.MeasureString("G:");
        int barWidth = Game1.uiViewport.Width / 7 - (int)rgbStrings.X;
        int barHeight = (int)(_colourWheel.Height / 6 / 1.5f);
        int barSpacing = 8;
        float leftEdge = _colourWheel.CenterPoint.X + barWidth / 2f + rgbStrings.X * 2.25f;
        float topEdge = _colourWheel.CenterPoint.Y - _colourWheel.Height / 2f;
        float bottomEdge = _colourWheel.CenterPoint.Y + _colourWheel.Height / 2f;

        Vector2 rBar = new Vector2(leftEdge, topEdge);
        Vector2 gBar = new Vector2(leftEdge, topEdge + barHeight + barSpacing);
        Vector2 bBar = new Vector2(leftEdge, topEdge + (barHeight + barSpacing) * 2);
        
        RedSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "R:", position: new Vector2(rBar.X - rgbStrings.X * 1.25f, rBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);

        BlueSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "G:", position: new Vector2(gBar.X - rgbStrings.X * 1.325f, gBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);

        GreenSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "B:", position: new Vector2(bBar.X - rgbStrings.X * 1.25f, bBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
        Vector2 hBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing) * 2);
        Vector2 sBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing));
        Vector2 vBar = new Vector2(leftEdge, bottomEdge - barHeight);

        HueSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "H:", position: new Vector2(hBar.X - rgbStrings.X * 1.25f, hBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
        SaturationSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "S:", position: new Vector2(sBar.X - rgbStrings.X * 1.25f, sBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
        ValueSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "V:", position: new Vector2(vBar.X - rgbStrings.X * 1.25f, vBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
    }

    public void drawSelectionCircle(SpriteBatch b)
    {
        Vector2 selectionPos = ColourWheel.RGBToPoint(SelectedColour);
        selectionPos = new Vector2(
            x: _colourWheel.CenterPoint.X + selectionPos.X * (_colourWheel.Width / 2f),
            y: _colourWheel.CenterPoint.Y + selectionPos.Y * (_colourWheel.Height / 2f)
        );
        Vector2 scale = new Vector2(2.5f, 2.5f);
        b.Draw(
            texture: _selectionCircle,
            position: selectionPos,
            sourceRectangle: null,
            color: Color.White * 0.8f,
            rotation: 0f,
            scale: scale,
            origin: new Vector2(_selectionCircle.Width / 2f, _selectionCircle.Height / 2f),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
    }

    public void drawSliderGrabbers(SpriteBatch b)
    {
        RedSlider.draw(b);
        BlueSlider.draw(b);
        GreenSlider.draw(b);
        HueSlider.draw(b);
        SaturationSlider.draw(b);
        ValueSlider.draw(b);

        // float grabberHeight = RedSlider.Bounds.Height / (float)_sliderGrabberMiddle.Height;
        //
        // Vector2 redGrabberPosition = new Vector2(RedSlider.Bounds.X + RedSlider.Bounds.Width * redValue, RedSlider.Bounds.Y);
        // drawSliderGrabber(b, redGrabberPosition, grabberHeight);
        //
        // Vector2 greenGrabberPosition = new Vector2(GreenSlider.Bounds.X + GreenSlider.Bounds.Width * greenValue, GreenSlider.Bounds.Y);
        // drawSliderGrabber(b, greenGrabberPosition, grabberHeight);
        //
        // Vector2 blueGrabberPosition = new Vector2(BlueSlider.Bounds.X + BlueSlider.Bounds.Width * blueValue, BlueSlider.Bounds.Y);
        // drawSliderGrabber(b, blueGrabberPosition, grabberHeight);
        //
        // Vector3 hsl = ColourWheel.RGBToHSV(SelectedColour);
        // float hueValue = hsl.X / 360.0f;
        // float saturationValue = hsl.Y;
        // float valueValue = hsl.Z;
        //
        // Vector2 hueGrabberPosition = new Vector2(HueSlider.Bounds.X + HueSlider.Bounds.Width * hueValue, HueSlider.Bounds.Y);
        // drawSliderGrabber(b, hueGrabberPosition, grabberHeight);
        //
        // Vector2 saturationGrabberPosition = new Vector2(SaturationSlider.Bounds.X + SaturationSlider.Bounds.Width * saturationValue, SaturationSlider.Bounds.Y);
        // drawSliderGrabber(b, saturationGrabberPosition, grabberHeight);
        //
        // Vector2 valueGrabberPosition = new Vector2(ValueSlider.Bounds.X + ValueSlider.Bounds.Width * valueValue, ValueSlider.Bounds.Y);
        // drawSliderGrabber(b, valueGrabberPosition, grabberHeight);
    }

    public override void update(GameTime time)
    {
        base.update(time);
        _value = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000f) * 0.5f + 0.5f;
        var smoothing = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 10f) * 0.5f + 0.5f;
        _colourWheel.Smoothing = smoothing;
        _colourWheel.Value = 1.0f;
        UpdateWheelCenter();
        UpdateSliders();
    }

    public void UpdateWheelCenter()
    {
        _colourWheel.CenterPoint = new Vector2(Game1.uiViewport.Width / 2f - Game1.uiViewport.Width / 13f, Game1.uiViewport.Height / 2f - Game1.uiViewport.Width / 13f);
        _colourWheel.Width = Game1.uiViewport.Width / 7f;
        _colourWheel.Height = Game1.uiViewport.Width / 7f;
    }

    public void UpdateSliders()
    {
        Vector2 rgbStrings = Game1.smallFont.MeasureString("G:");
        int barWidth = Game1.uiViewport.Width / 7 - (int)rgbStrings.X;
        int barHeight = (int)(_colourWheel.Height / 6 / 1.5f);
        int barSpacing = 8;
        float leftEdge = _colourWheel.CenterPoint.X + barWidth / 2f + rgbStrings.X * 2.25f;
        float topEdge = _colourWheel.CenterPoint.Y - _colourWheel.Height / 2f;
        float bottomEdge = _colourWheel.CenterPoint.Y + _colourWheel.Height / 2f;

        Vector2 rBar = new Vector2(leftEdge, topEdge);
        Vector2 gBar = new Vector2(leftEdge, topEdge + barHeight + barSpacing);
        Vector2 bBar = new Vector2(leftEdge, topEdge + (barHeight + barSpacing) * 2);
        
        float redValue = SelectedColour.R / 255f;
        float greenValue = SelectedColour.G / 255f;
        float blueValue = SelectedColour.B / 255f;
        
        RedSlider.UpdateBarBounds(new Rectangle((int)rBar.X, (int)rBar.Y, width: barWidth, height: barHeight));
        RedSlider.UpdateColours(new Color(0, SelectedColour.G, SelectedColour.B), new Color(255, SelectedColour.G, SelectedColour.B));
        RedSlider.UpdateProgress(redValue);
        
        GreenSlider.UpdateBarBounds(new Rectangle((int)gBar.X, (int)gBar.Y, width: barWidth, height: barHeight));
        GreenSlider.UpdateColours(new Color(SelectedColour.R, 0, SelectedColour.B), new Color(SelectedColour.R, 255, SelectedColour.B));
        GreenSlider.UpdateProgress(greenValue);
        
        BlueSlider.UpdateBarBounds(new Rectangle((int)bBar.X, (int)bBar.Y, width: barWidth, height: barHeight));
        BlueSlider.UpdateColours(new Color(SelectedColour.R, SelectedColour.G, 0), new Color(SelectedColour.R, SelectedColour.G, 255));
        BlueSlider.UpdateProgress(blueValue);
        
        // RedSlider = new GradientBar(bounds: new Rectangle((int)rBar.X, (int)rBar.Y, width: barWidth, height: barHeight), colourOne: new Color(0, SelectedColour.G, SelectedColour.B), colourTwo: new Color(255, SelectedColour.G, SelectedColour.B));
        // GreenSlider = new GradientBar(bounds: new Rectangle((int)gBar.X, (int)gBar.Y, width: barWidth, height: barHeight), colourOne: new Color(SelectedColour.R, 0, SelectedColour.B), colourTwo: new Color(SelectedColour.R, 255, SelectedColour.B));
        // BlueSlider = new GradientBar(bounds: new Rectangle((int)bBar.X, (int)bBar.Y, width: barWidth, height: barHeight), colourOne: new Color(SelectedColour.R, SelectedColour.G, 0), colourTwo: new Color(SelectedColour.R, SelectedColour.G, 255));
        
        Vector2 hBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing) * 2);
        Vector2 sBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing));
        Vector2 vBar = new Vector2(leftEdge, bottomEdge - barHeight);
        
        Vector3 hsl = ColourWheel.RGBToHSV(SelectedColour);
        float hueValue = hsl.X / 360.0f;
        float saturationValue = hsl.Y;
        float valueValue = hsl.Z;
        
        HueSlider.UpdateBarBounds(new Rectangle((int)hBar.X, (int)hBar.Y, width: barWidth, height: barHeight));
        HueSlider.UpdateProgress(hueValue);
        
        SaturationSlider.UpdateBarBounds(new Rectangle((int)sBar.X, (int)sBar.Y, width: barWidth, height: barHeight));
        SaturationSlider.UpdateColours(Color.White, SelectedColour);
        SaturationSlider.UpdateProgress(saturationValue);
        
        ValueSlider.UpdateBarBounds(new Rectangle((int)vBar.X, (int)vBar.Y, width: barWidth, height: barHeight));
        ValueSlider.UpdateColours(Color.Black, SelectedColour);
        ValueSlider.UpdateProgress(valueValue);
        
        // HueSlider = new GradientBar(bounds: new Rectangle((int)hBar.X, (int)hBar.Y, width: barWidth, height: barHeight), isHueBar: true);
        // SaturationSlider = new GradientBar(bounds: new Rectangle((int)sBar.X, (int)sBar.Y, width: barWidth, height: barHeight), colourOne: Color.White, colourTwo: SelectedColour);
        // ValueSlider = new GradientBar(bounds: new Rectangle((int)vBar.X, (int)vBar.Y, width: barWidth, height: barHeight), colourOne: Color.Black, colourTwo: SelectedColour);
    }

    public void UpdateSliderGrabbers()
    {
        
    }
}