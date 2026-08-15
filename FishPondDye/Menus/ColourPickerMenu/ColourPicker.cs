using System;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPicker : IClickableMenu
{
    private static readonly Texture2D _selectionCircle = GenerateSelectionOutline();
    
    private FishPond? _pond = null;
    
    public Vector2 menuCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2f);

    private readonly ColourWheel _colourWheel = new(
        new Vector2(
            x: Game1.uiViewport.Width / 2f,
            y: Game1.uiViewport.Height / 3f),
        width: (int)(Game1.uiViewport.Height / 4f),
        height: (int)(Game1.uiViewport.Height / 4f))
    {
        Smoothing = 1.0f
    };
    
    private decimal _red => _pickedColour.ToRgb().R;
    private decimal _green => _pickedColour.ToRgb().G;
    private decimal _blue => _pickedColour.ToRgb().B;
    
    private decimal _hue = 0;
    private decimal _saturation = 0;
    private decimal _value = 100;

    private float _alpha = 1f;
    
    private HsvColour _pickedColour => new(_hue, _saturation, _value);
    private HsvColour _pickedColourMaxValue => new(_hue, _saturation, 100);

    private bool _isSelecting = false;
    
    private ColourSlider RedSlider;
    private ColourSlider BlueSlider;
    private ColourSlider GreenSlider;
    private ColourSlider HueSlider;
    private ColourSlider SaturationSlider;
    private ColourSlider ValueSlider;
    private ColourSlider AlphaSlider;

    private ColourSlider SelectedColourPreview; // Just reusing the ColourSlider because it has all the border drawing code already.

    private HexInput _hexInput;

    public ColourPicker(FishPond? pond)
    {
        _pond = pond;

        width = Game1.uiViewport.Width / 2;
        height = Game1.uiViewport.Height / 2;
        
        RedSlider = new ColourSlider(getter: () => GetR() / 255);
        GreenSlider = new ColourSlider(getter: () => GetG() / 255);
        BlueSlider = new ColourSlider(getter: () => GetB() / 255);
        HueSlider = new ColourSlider(getter: () => GetHue() / 360) { IsHueBar = true };
        SaturationSlider = new ColourSlider(getter: () => GetSaturation() / 100);
        ValueSlider = new ColourSlider(getter: () => GetValue() / 100);
        AlphaSlider = new ColourSlider(getter: GetAlpha) { IsAlphaBar = true };
        
        SelectedColourPreview = new ColourSlider(getter: () => -1);
        
        _hexInput = new HexInput(Game1.dialogueFont, Game1.textColor)
        {
            X = Game1.uiViewport.Width / 2 - 256 - 32 - spaceToClearSideBorder / 2,
            Y = Game1.uiViewport.Height / 2,
            Width = 0,
            Height = 0,
            limitWidth = false,
            textLimit = 8,
            numbersOnly = false
        };
        
        // _hexInput.OnEnterPressed += textBoxEnter;
        Game1.keyboardDispatcher.Subscriber = _hexInput;
        _hexInput.Text = "FFFFFF";
    }

    public Color GetCurrentColour()
    {
        return _pickedColour.ToRgb().ToXnaColor() * _alpha;
    }

    private void SetR(decimal r)
    {
        RgbColour rgb = _pickedColour.ToRgb();
        HsvColour newColour = new RgbColour(r, rgb.G, rgb.B).ToHsv();
        SetHue(newColour.H);
        SetSaturation(newColour.S);
        SetValue(newColour.V);
    }
    
    private void SetG(decimal g)
    {
        RgbColour rgb = _pickedColour.ToRgb();
        HsvColour newColour = new RgbColour(rgb.R, g, rgb.B).ToHsv();
        SetHue(newColour.H);
        SetSaturation(newColour.S);
        SetValue(newColour.V);
    }
    
    private void SetB(decimal b)
    {
        RgbColour rgb = _pickedColour.ToRgb();
        HsvColour newColour = new RgbColour(rgb.R, rgb.G, b).ToHsv();
        SetHue(newColour.H);
        SetSaturation(newColour.S);
        SetValue(newColour.V);
    }
    
    private void SetHue(decimal h)
    {
        _hue = h;
    }

    private void SetSaturation(decimal s)
    {
        _saturation = s;
    }

    private void SetValue(decimal v)
    {
        _value = v;
    }

    private void SetAlpha(float a)
    {
        _alpha = a;
    }
    
    private decimal GetR() => _red;
    private decimal GetG() => _green;
    private decimal GetB() => _blue;
    private decimal GetHue() => _hue;
    private decimal GetSaturation() => _saturation;
    private decimal GetValue() => _value;
    private decimal GetAlpha() => (decimal)_alpha;
    
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
        AlphaSlider.IsSelected = false;
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        if (_colourWheel.IsSelected)
        {
            RgbColour rgb = _colourWheel.GetColourAtScreenPoint(new Vector2(x, y));
            HsvColour newColour = ColourWheel.PointToHsv(_colourWheel.ScreenPointToUnitSpace(new Vector2(x, y)));
            SetHue(newColour.H);
            SetSaturation(newColour.S);
            SetValue(_value);
        }
        
        if (RedSlider.IsSelected)
        {
            decimal progress = (x - RedSlider.Bar.Bounds.X) / (decimal)RedSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetR(progress * 255);
        }
        if (GreenSlider.IsSelected)
        {
            decimal progress = (x - GreenSlider.Bar.Bounds.X) / (decimal)GreenSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetG(progress * 255);
        }
        if (BlueSlider.IsSelected)
        {
            decimal progress = (x - BlueSlider.Bar.Bounds.X) / (decimal)BlueSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetB(progress * 255);
        }
        if (HueSlider.IsSelected)
        {
            decimal progress = (x - HueSlider.Bar.Bounds.X) / (decimal)HueSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetHue(progress * 360);
        }
        if (SaturationSlider.IsSelected)
        {
            decimal progress = (x - SaturationSlider.Bar.Bounds.X) / (decimal)SaturationSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetSaturation(progress * 100);
        }
        if (ValueSlider.IsSelected)
        {
            decimal progress = (x - ValueSlider.Bar.Bounds.X) / (decimal)ValueSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetValue(progress * 100);
        }
        if (AlphaSlider.IsSelected)
        {
            float progress = (x - AlphaSlider.Bar.Bounds.X) / (float)AlphaSlider.Bar.Bounds.Width;
            progress = Math.Clamp(progress, 0, 1);
            SetAlpha(progress);
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
        AlphaSlider.IsSelected = AlphaSlider.ContainsPoint(new Point(x, y));
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
            x: (int)(menuCenter.X - width / 2f),
            y: (int)(menuCenter.Y - height / 2f),
            width: width,
            height: height
        );
        
        _colourWheel.draw(b);
        drawSelectionCircle(b);
        drawRgbBars(b);
        drawSliderGrabbers(b);
        _hexInput.Draw(b);
        
        drawMouse(b);
    }

    public void drawRgbBars(SpriteBatch b)
    {
        float referenceHeight = _colourWheel.Height * 1.35f;
        Vector2 rgbStrings = Game1.smallFont.MeasureString("G:");
        int barWidth = Game1.uiViewport.Width / 7 - (int)rgbStrings.X;
        int barHeight = (int)(referenceHeight / (6 * 1.35f) / 1.5f);
        int barSpacing = 12;
        float leftEdge = _colourWheel.CenterPoint.X + barWidth / 2f + rgbStrings.X * 2.25f;
        float topEdge = _colourWheel.CenterPoint.Y - referenceHeight / (2 * 1.35f);
        float bottomEdge = _colourWheel.CenterPoint.Y + referenceHeight / 2f;

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
        
        float inputTopEdge = bBar.Y + barHeight + barSpacing;
        float inputBottomEdge = hBar.Y - barSpacing;
        Vector2 aBar = new Vector2(leftEdge, inputTopEdge + (inputBottomEdge - inputTopEdge) / 2f - barHeight / 2f);
        
        AlphaSlider.draw(b);
        b.DrawString(spriteFont: Game1.smallFont, text: "A:", position: new Vector2(aBar.X - rgbStrings.X * 1.125f, aBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
        SelectedColourPreview.draw(b);
    }

    public void drawSelectionCircle(SpriteBatch b)
    {
        Vector2 selectionPos = ColourWheel.HsvToPoint(_pickedColourMaxValue);
        selectionPos = new Vector2(
            x: _colourWheel.CenterPoint.X + (selectionPos.X) * (_colourWheel.Width / 2f),
            y: _colourWheel.CenterPoint.Y + (selectionPos.Y) * (_colourWheel.Height / 2f)
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
        UpdateTextBox();
    }

    public void UpdateWheelCenter()
    {
        _colourWheel.CenterPoint = new Vector2(Game1.uiViewport.Width / 2f - Game1.uiViewport.Width / 13f, Game1.uiViewport.Height / 2f - Game1.uiViewport.Width / 13f);
        _colourWheel.Width = Game1.uiViewport.Width / 7f;
        _colourWheel.Height = Game1.uiViewport.Width / 7f;
    }

    public void UpdateTextBox()
    {
        // // Place the text box between the RGB and HSV sliders
        // Vector2 rgbStrings = Game1.smallFont.MeasureString("G:");
        // int barWidth = Game1.uiViewport.Width / 7 - (int)rgbStrings.X;
        // int barHeight = (int)(_colourWheel.Height / 6 / 1.5f);
        // int barSpacing = 8;
        //
        // float leftEdge = _colourWheel.CenterPoint.X + barWidth / 2f + rgbStrings.X * 2.25f;
        // float topEdge = _colourWheel.CenterPoint.Y - _colourWheel.Height / 2f + (barHeight + 8) * 4;
        // float bottomEdge = _colourWheel.CenterPoint.Y + _colourWheel.Height / 2f;
        //
        // _hexInput.X = (int)leftEdge;
        // _hexInput.Y = (int)(topEdge + (bottomEdge - topEdge) / 2f - barHeight / 2f);
        // _hexInput.Width = (int)(barWidth * 0.75f);
        // _hexInput.Height = barHeight;
    }

    public void UpdateSliders()
    {
        float referenceHeight = _colourWheel.Height * 1.35f;
        Vector2 rgbStrings = Game1.smallFont.MeasureString("G:");
        int barWidth = Game1.uiViewport.Width / 7 - (int)rgbStrings.X;
        int barHeight = (int)(referenceHeight / (6 * 1.35f) / 1.5f);
        int barSpacing = 12;
        float leftEdge = _colourWheel.CenterPoint.X + barWidth / 2f + rgbStrings.X * 2.25f;
        float topEdge = _colourWheel.CenterPoint.Y - referenceHeight / (2 * 1.35f);
        float bottomEdge = _colourWheel.CenterPoint.Y + referenceHeight / (2f);
        
        Vector2 rBar = new Vector2(leftEdge, topEdge);
        Vector2 gBar = new Vector2(leftEdge, topEdge + barHeight + barSpacing);
        Vector2 bBar = new Vector2(leftEdge, topEdge + (barHeight + barSpacing) * 2);
        
        Color currentColour = GetCurrentColour();
        RedSlider.UpdateBarBounds(new Rectangle((int)rBar.X, (int)rBar.Y, width: barWidth, height: barHeight));
        RedSlider.UpdateColours(new Color(0, currentColour.G, currentColour.B), new Color(255, currentColour.G, currentColour.B));
        
        GreenSlider.UpdateBarBounds(new Rectangle((int)gBar.X, (int)gBar.Y, width: barWidth, height: barHeight));
        GreenSlider.UpdateColours(new Color(currentColour.R, 0, currentColour.B), new Color(currentColour.R, 255, currentColour.B));
        
        BlueSlider.UpdateBarBounds(new Rectangle((int)bBar.X, (int)bBar.Y, width: barWidth, height: barHeight));
        BlueSlider.UpdateColours(new Color(currentColour.R, currentColour.G, 0), new Color(currentColour.R, currentColour.G, 255));
        
        Vector2 hBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing) * 2);
        Vector2 sBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing));
        Vector2 vBar = new Vector2(leftEdge, bottomEdge - barHeight);
        
        HueSlider.UpdateBarBounds(new Rectangle((int)hBar.X, (int)hBar.Y, width: barWidth, height: barHeight));
        
        SaturationSlider.UpdateBarBounds(new Rectangle((int)sBar.X, (int)sBar.Y, width: barWidth, height: barHeight));
        SaturationSlider.UpdateColours(Color.White, currentColour);
        
        ValueSlider.UpdateBarBounds(new Rectangle((int)vBar.X, (int)vBar.Y, width: barWidth, height: barHeight));
        ValueSlider.UpdateColours(Color.Black, _pickedColourMaxValue.ToXnaColor());
        
        // // We also position the _hexInput here too since it goes between the RGB and HSV sliders and we've already got all the edges and whatnot calculated.
        float inputTopEdge = bBar.Y + barHeight + barSpacing;
        float inputBottomEdge = hBar.Y - barSpacing;
        
        Vector2 aBar = new Vector2(leftEdge, inputTopEdge + (inputBottomEdge - inputTopEdge) / 2f - barHeight / 2f);
        AlphaSlider.UpdateBarBounds(new Rectangle((int)aBar.X, (int)aBar.Y, width: barWidth, height: barHeight));
        AlphaSlider.UpdateColours(Color.Transparent, new Color(currentColour.R, currentColour.G, currentColour.B));
        
        SelectedColourPreview.UpdateBarBounds(new Rectangle(
            x: (int)(_colourWheel.CenterPoint.X - (_colourWheel.Width * 0.98f) / 2f),
            y: (int)(vBar.Y),
            width: (int)(_colourWheel.Width * 0.98f),
            height: barHeight
        ));
        SelectedColourPreview.UpdateColours(currentColour, currentColour);
        
        // Vector2 inputBar = new Vector2(leftEdge, inputTopEdge + (inputBottomEdge - inputTopEdge) / 2f - barHeight / 2f);
        //
        // _hexInput.X = (int)inputBar.X;
        // _hexInput.Y = (int)inputBar.Y;
        // _hexInput.Width = barWidth;
        // _hexInput.Height = barHeight;
    }
}