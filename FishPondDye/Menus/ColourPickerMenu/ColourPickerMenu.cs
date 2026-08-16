using System;
using System.Collections.Generic;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu : IClickableMenu
{
    private static Texture2D _selectionCircle
    {
        get
        {
            if (field is not null) return field;
            
            field = new Texture2D(Game1.graphics.GraphicsDevice, 9, 9);
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
            field.SetData(data);
            return field;
        }
    }
    private Vector2 _screenCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2f);

    private ColourWheel _colourWheel;

    private decimal _hue;
    private decimal _saturation;
    private decimal _value;
    private decimal _alpha;

    private readonly List<ColourSlider> _sliders = [];
    private ColourSlider? _selectedSlider = null;
    
    public HsvColour PickedColourHsv => new(_hue, _saturation, _value, _alpha);
    public RgbColour PickedColourRgb => PickedColourHsv.ToRgb();

    private FishPond? _pond = null;

    public ColourPickerMenu(FishPond? pond)
    {
        _pond = pond;
        width = Game1.uiViewport.Width / 4;
        height = Game1.uiViewport.Width / 3;
        
        _colourWheel = new ColourWheel(
            centerPoint: new Vector2(_screenCenter.X, _screenCenter.Y - height / 6f),
            width: width,
            height: width
        );
        
        _sliders.Add(
            new ColourSlider(
                name: "Red",
                getBackingValue: GetRed,
                setBackingValue: SetRed,
                min: 0,
                max: 255,
                bounds: new Rectangle(
                    x: (int)_screenCenter.X - width / 2,
                    y: (int)_screenCenter.Y + height / 6,
                    width: width,
                    height: 20
                ),
                colourOne: Color.White,
                colourTwo: Color.Red
            )
        );
        //
        // _sliders.Add(
        //     new ColourSlider(
        //         getBackingValue: GetGreen,
        //         setBackingValue: SetGreen,
        //         min: 0,
        //         max: 255,
        //         bounds: new Rectangle(
        //             x: (int)_screenCenter.X - width / 2,
        //             y: (int)_screenCenter.Y + height / 6 + 30,
        //             width: width,
        //             height: 20
        //         ),
        //         colourOne: Color.White,
        //         colourTwo: Color.Green
        //     )
        // );
        //
        // _sliders.Add(
        //     new ColourSlider(
        //         getBackingValue: GetBlue,
        //         setBackingValue: SetBlue,
        //         min: 0,
        //         max: 255,
        //         bounds: new Rectangle(
        //             x: (int)_screenCenter.X - width / 2,
        //             y: (int)_screenCenter.Y + height / 6 + 60,
        //             width: 20,
        //             height: 200
        //         ),
        //         colourOne: Color.Blue,
        //         colourTwo: Color.White
        //     ) { IsHorizontal = false }
        // );
    }

    public decimal GetRed()
    {
        return PickedColourRgb.Red;
    }
    
    public decimal GetGreen()
    {
        return PickedColourRgb.Green;
    }
    
    public decimal GetBlue()
    {
        return PickedColourRgb.Blue;
    }

    public void SetRed(decimal red)
    {
        RgbColour rgb = new RgbColour(red, GetGreen(), GetBlue());
        SetColour(rgb);
    }
    
    public void SetGreen(decimal green)
    {
        RgbColour rgb = new RgbColour(GetRed(), green, GetBlue());
        SetColour(rgb);
    }
    
    public void SetBlue(decimal blue)
    {
        RgbColour rgb = new RgbColour(GetRed(), GetGreen(), blue);
        SetColour(rgb);
    }

    public void SetColour(RgbColour rgb)
    {
        HsvColour hsv = rgb.ToHsv();
        SetColour(hsv);
    }

    public void SetColour(HsvColour hsv)
    {
        _hue = hsv.Hue;
        _saturation = hsv.Saturation;
        _value = hsv.Value;
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        width = Game1.uiViewport.Width / 4;
        height = Game1.uiViewport.Width / 3;
        float topEdge = _screenCenter.Y - height / 2f;
        _colourWheel.CenterPoint = new Vector2(_screenCenter.X, topEdge + width / 2f - borderWidth / 2f);
        _colourWheel.Width = _colourWheel.Height = width;
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        _colourWheel.Selected = false;
        foreach (var slider in _sliders)
        {
            slider.Selected = false;
        }
        _selectedSlider = null;
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);

        if (_colourWheel.Selected)
        {
            HsvColour hsv = _colourWheel.GetColourAtScreenPoint(new Vector2(x, y)).ToHsv();
            _hue = hsv.Hue;
            _saturation = hsv.Saturation;
            _value = hsv.Value;
        }
        
        if (_selectedSlider is not null)
        {
            float progress;
            if (_selectedSlider.IsHorizontal)
            {
                progress = (float)(x - _selectedSlider.Bar.Bounds.X) / _selectedSlider.Bar.Bounds.Width;
            }
            else
            {
                progress = (float)(y - _selectedSlider.Bar.Bounds.Y) / _selectedSlider.Bar.Bounds.Height;
            }
            _selectedSlider.Progress = Math.Clamp(progress, 0f, 1f);
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        _colourWheel.Selected = _colourWheel.Contains(new Vector2(x, y));
        if (_colourWheel.Selected) return;
        
        foreach (var slider in _sliders)
        {
            slider.Selected = slider.containsPoint(x, y);
            if (!slider.Selected) continue;
            
            _selectedSlider = slider;
            break;
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.4f);
        
        Game1.DrawBox(
            x: (int)_screenCenter.X - width / 2 - borderWidth,
            y: (int)_screenCenter.Y - height / 2 - borderWidth,
            width: width + borderWidth * 2,
            height: height + borderWidth * 2
        );

        _colourWheel.draw(b);
        drawSelectionCircle(b);
        
        foreach (var slider in _sliders)
        {
            slider.draw(b);
        }

        drawMouse(b);
    }

    public void drawSelectionCircle(SpriteBatch b)
    {
        Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
        Vector2 position = new Vector2(
            x: _colourWheel.CenterPoint.X + point.X * _colourWheel.Width / 2f,
            y: _colourWheel.CenterPoint.Y + point.Y * _colourWheel.Height / 2f
        );
        b.Draw(
            texture: _selectionCircle,
            position: position,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: new Vector2(4.5f, 4.5f),
            scale: 2f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
    }

    public override void update(GameTime time)
    {
        base.update(time);
        
        gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
    }
}