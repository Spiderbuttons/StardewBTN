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
    private static readonly Texture2D _selectionCircle = GenerateSelectionOutline();
    
    private FishPond? _pond = null;

    private readonly ColourWheel _colourWheel = new(
        new Vector2(
            x: Game1.uiViewport.Width / 2f,
            y: Game1.uiViewport.Height / 3f), 
        width: (int)(Game1.uiViewport.Height / 4f), 
        height: (int)(Game1.uiViewport.Height / 4f));

    private float _red = 1;
    private float _green = 1;
    private float _blue = 1;
    
    private float _hue => ColourWheel.RGBToHSV(ColourWithoutValue).X;
    private float _saturation => ColourWheel.RGBToHSV(ColourWithoutValue).Y;
    private float _value = 1;
    
    public Color ColourWithoutValue => new(_red, _green, _blue);
    public Color SelectedColour => Color.Lerp(Color.Black, ColourWithoutValue, _value);

    private bool _isSelecting = false;
    
    private ColourSlider RedSlider;
    private ColourSlider BlueSlider;
    private ColourSlider GreenSlider;
    private ColourSlider HueSlider;
    private ColourSlider SaturationSlider;
    private ColourSlider ValueSlider;

    public ColourPicker(FishPond? pond)
    {
        _pond = pond;
        RedSlider = new ColourSlider(getter: GetR, setter: SetR);
        GreenSlider = new ColourSlider(getter: GetG, setter: SetG);
        BlueSlider = new ColourSlider(getter: GetB, setter: SetB);
        HueSlider = new ColourSlider(getter: GetHue, setter: SetHue, isHueBar: true);
        SaturationSlider = new ColourSlider(getter: GetSaturation, setter: SetSaturation);
        ValueSlider = new ColourSlider(getter: GetValue, setter: SetValue);
    }

    private void SetR(float r)
    {
        _red = r;
    }
    
    private void SetG(float g)
    {
        _green = g;
    }
    
    private void SetB(float b)
    {
        _blue = b;
    }
    
    private void SetHue(float h)
    {
        var hsl = ColourWheel.RGBToHSV(ColourWithoutValue);
        hsl.X = h * 360f;
        Color rgb = ColourWheel.HSVtoRGB(hsl);
        _red = rgb.R / 255f;
        _green = rgb.G / 255f;
        _blue = rgb.B / 255f;
    }

    private void SetSaturation(float s)
    {
        var hsl = ColourWheel.RGBToHSV(ColourWithoutValue);
        hsl.Y = s;
        Color rgb = ColourWheel.HSVtoRGB(hsl);
        _red = rgb.R / 255f;
        _green = rgb.G / 255f;
        _blue = rgb.B / 255f;
    }

    private void SetValue(float v)
    {
        _value = v;
    }
    
    public void SetColourWithoutValue(Color colour)
    {
        _red = colour.R / 255f;
        _green = colour.G / 255f;
        _blue = colour.B / 255f;
    }
    
    private float GetR() => _red;
    private float GetG() => _green;
    private float GetB() => _blue;
    private float GetHue() => _hue / 360f;
    private float GetSaturation() => _saturation;
    private float GetValue() => _value;
    
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        _colourWheel.IsSelected = false;
        RedSlider.IsSelected = false;
        GreenSlider.IsSelected = false;
        BlueSlider.IsSelected = false;
        HueSlider.IsSelected = false;
        SaturationSlider.IsSelected = false;
        ValueSlider.IsSelected = false;
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        if (_colourWheel.IsSelected) SetColourWithoutValue(_colourWheel.GetColourAtPoint(new Vector2(x, y)));
        
        if (RedSlider.IsSelected)
        {
            float progress = (x - RedSlider.Bar.Bounds.X) / (float)RedSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0f, 1f);
            SetR(progress);
        }
        if (GreenSlider.IsSelected)
        {
            float progress = (x - GreenSlider.Bar.Bounds.X) / (float)GreenSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0f, 1f);
            SetG(progress);
        }
        if (BlueSlider.IsSelected)
        {
            float progress = (x - BlueSlider.Bar.Bounds.X) / (float)BlueSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0f, 1f);
            SetB(progress);
        }
        
        if (HueSlider.IsSelected)
        {
            float progress = (x - HueSlider.Bar.Bounds.X) / (float)HueSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0f, 1f);
            SetHue(progress);
        }
        if (SaturationSlider.IsSelected)
        {
            float progress = (x - SaturationSlider.Bar.Bounds.X) / (float)SaturationSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0f, 1f);
            SetSaturation(progress);
        }
        if (ValueSlider.IsSelected)
        {
            float progress = (x - ValueSlider.Bar.Bounds.X) / (float)ValueSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0f, 1f);
            SetValue(progress);
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        _colourWheel.IsSelected = _colourWheel.Contains(new Vector2(x, y));
        RedSlider.IsSelected = RedSlider.ContainsPoint(new Point(x, y));
        GreenSlider.IsSelected = GreenSlider.ContainsPoint(new Point(x, y));
        BlueSlider.IsSelected = BlueSlider.ContainsPoint(new Point(x, y));
        HueSlider.IsSelected = HueSlider.ContainsPoint(new Point(x, y));
        SaturationSlider.IsSelected = SaturationSlider.ContainsPoint(new Point(x, y));
        ValueSlider.IsSelected = ValueSlider.ContainsPoint(new Point(x, y));
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
        Vector2 selectionPos = ColourWheel.RGBToPoint(ColourWithoutValue);
        selectionPos = new Vector2(
            x: _colourWheel.CenterPoint.X + selectionPos.X * (_colourWheel.Width / 2f),
            y: _colourWheel.CenterPoint.Y + selectionPos.Y * (_colourWheel.Height / 2f)
        );
        Vector2 scale = new Vector2(2f, 2f);
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
    }

    public override void update(GameTime time)
    {
        base.update(time);
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
        
        RedSlider.UpdateBarBounds(new Rectangle((int)rBar.X, (int)rBar.Y, width: barWidth, height: barHeight));
        RedSlider.UpdateColours(new Color(0, SelectedColour.G, SelectedColour.B), new Color(255, SelectedColour.G, SelectedColour.B));
        
        GreenSlider.UpdateBarBounds(new Rectangle((int)gBar.X, (int)gBar.Y, width: barWidth, height: barHeight));
        GreenSlider.UpdateColours(new Color(SelectedColour.R, 0, SelectedColour.B), new Color(SelectedColour.R, 255, SelectedColour.B));
        
        BlueSlider.UpdateBarBounds(new Rectangle((int)bBar.X, (int)bBar.Y, width: barWidth, height: barHeight));
        BlueSlider.UpdateColours(new Color(SelectedColour.R, SelectedColour.G, 0), new Color(SelectedColour.R, SelectedColour.G, 255));
        
        Vector2 hBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing) * 2);
        Vector2 sBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing));
        Vector2 vBar = new Vector2(leftEdge, bottomEdge - barHeight);
        
        HueSlider.UpdateBarBounds(new Rectangle((int)hBar.X, (int)hBar.Y, width: barWidth, height: barHeight));
        
        SaturationSlider.UpdateBarBounds(new Rectangle((int)sBar.X, (int)sBar.Y, width: barWidth, height: barHeight));
        SaturationSlider.UpdateColours(Color.White, SelectedColour);
        
        ValueSlider.UpdateBarBounds(new Rectangle((int)vBar.X, (int)vBar.Y, width: barWidth, height: barHeight));
        ValueSlider.UpdateColours(Color.Black, SelectedColour);
    }
}