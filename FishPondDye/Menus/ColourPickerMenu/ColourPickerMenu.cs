using System;
using System.Collections.Generic;
using System.Linq;
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
    private Vector2 _screenCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2.25f);

    private Vector2 _leftSectionOffset = Vector2.Zero;
    
    private Vector2 _rightSectionOffset = Vector2.Zero;
    private Vector2 _rightSectionCenter => _colourWheel.CenterPoint + _rightSectionOffset;

    private ColourWheel _colourWheel;

    private decimal _hue;
    private decimal _saturation;
    private decimal _value = 100M;
    private decimal _alpha = 100M;

    private readonly Dictionary<string, ColourSlider> _sliders = [];
    private ColourSlider? _selectedSlider = null;
    
    private int _paletteSquaresPerRow = 12;
    private int _minimumPaletteSquareGap => width / 72;
    private List<PaletteSquare> _palette = [];
    
    public HsvColour PickedColourHsv => new(_hue, _saturation, _value, _alpha);
    public RgbColour PickedColourRgb => PickedColourHsv.ToRgb();

    // Reusing the ColourSlider because it already draws the border I want.
    private ColourSlider PickedColourSlider = new(
        name: "PickedColourSlider",
        getBackingValue: () => -1,
        setBackingValue: _ => { },
        min: 0,
        max: 1,
        bounds: new Rectangle(0, 0, 0, 0),
        colourOne: Color.White,
        colourTwo: Color.White
    ) { IsHorizontal = true };

    private FishPond? _pond = null;

    public ColourPickerMenu(FishPond? pond)
    {
        _pond = pond;
        width = Game1.uiViewport.Width / 4;
        height = Game1.uiViewport.Width / 4;
        
        _colourWheel = new ColourWheel(
            name: "ColourWheel",
            centerPoint: new Vector2(_screenCenter.X, _screenCenter.Y - height / 6f),
            width: width,
            height: width
        );

        int squaresPerRow = _paletteSquaresPerRow;
        int maxWidthForPaletteSquares = width / squaresPerRow;
        
        // The minimum gap between squares should be (width / 48) because I said so. But I also want the gap to be as large as it possibly can be while still fitting all the squares per row inside the overall width. So this just keeps increasing the gap until it's so large that all the squares wouldn't be able to fit anymore.
        int gap;
        while (true)
        {
            gap = (width - squaresPerRow * maxWidthForPaletteSquares) / (squaresPerRow - 1);
            if (gap >= _minimumPaletteSquareGap) break;
            maxWidthForPaletteSquares--;
        }
        
        // If we can't perfectly distribute the squares and gaps, we want to at least center the row of squares instead.
        int startingX = (int)_screenCenter.X - width / 2;
        int offset = (width - (squaresPerRow * maxWidthForPaletteSquares + (squaresPerRow - 1) * gap)) / 2;
        startingX += offset;

        List<Color>? savedPalette = null;
        if (Game1.player.modData.TryGetValue("Spiderbuttons.SpiderUI.ColourPickerPalette", out string? paletteString))
        {
            if (!string.IsNullOrWhiteSpace(paletteString)) {
                savedPalette = paletteString.Split(' ')
                    .Select(s => new Color(uint.Parse(s)))
                    .ToList();
            }
        }
        
        for (int i = 0; i < squaresPerRow * 2; i++)
        {
            Color? colour = i switch 
            {
                0 => Color.Black,
                1 => Color.White,
                2 => Color.Red,
                3 => Color.Green,
                4 => Color.Blue,
                5 => Color.Cyan,
                6 => Color.Magenta,
                7 => Color.Yellow,
                8 => new Color(255, 128, 0),
                9 => new Color(160, 0, 255),
                10 => new Color(0, 255, 0),
                11 => new Color(255, 0, 169),
                _ => null
            };
            
            if (savedPalette is not null && i - 1 >= squaresPerRow)
            {
                int savedIndex = i - 1 - squaresPerRow;
                if (savedIndex < savedPalette.Count)
                {
                    colour = savedPalette[savedIndex];
                }
            }
            
            int x = startingX + i * (maxWidthForPaletteSquares + gap);
            int y = (int)_screenCenter.Y + height / 3 + maxWidthForPaletteSquares + gap * 2;
            if (i >= squaresPerRow)
            {
                x = startingX + (i - squaresPerRow) * (maxWidthForPaletteSquares + gap);
                y += maxWidthForPaletteSquares + gap;
            }
            
            _palette.Add(new PaletteSquare(
                name: $"PaletteSquare{i}",
                bounds: new Rectangle(x, y, maxWidthForPaletteSquares, maxWidthForPaletteSquares),
                storedColour: colour,
                locked: i < squaresPerRow
            )
            {
                scale = maxWidthForPaletteSquares / 10f,
                IsAddSquare = i == squaresPerRow
            });
        }

        _sliders["Red"] = new ColourSlider(
            name: "Red",
            getBackingValue: GetRed,
            setBackingValue: SetRed,
            min: 0,
            max: 255,
            bounds: new Rectangle(
                x: (int)_screenCenter.X - width / 2,
                y: (int)_screenCenter.Y + height / 4,
                width: width,
                height: 20
            ),
            colourOne: Color.White,
            colourTwo: Color.Red
        );
        
        _sliders["Green"] = new ColourSlider(
            name: "Green",
            getBackingValue: GetGreen,
            setBackingValue: SetGreen,
            min: 0,
            max: 255,
            bounds: new Rectangle(
                x: (int)_screenCenter.X - width / 2,
                y: (int)_screenCenter.Y + height / 4 + 30,
                width: width,
                height: 20
            ),
            colourOne: Color.White,
            colourTwo: new Color(0, 255, 0)
        );
        
        _sliders["Blue"] = new ColourSlider(
            name: "Blue",
            getBackingValue: GetBlue,
            setBackingValue: SetBlue,
            min: 0,
            max: 255,
            bounds: new Rectangle(
                x: (int)_screenCenter.X - width / 2,
                y: (int)_screenCenter.Y + height / 4 + 60,
                width: 20,
                height: 200
            ),
            colourOne: Color.White,
            colourTwo: Color.Blue
        );
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
    
    public void AddColourToPalette(HsvColour hsv)
    {
        Color colour = hsv.ToXnaColor();
        for (var i = _palette.Count - 1; i > _paletteSquaresPerRow; i--)
        {
            _palette[i].StoredColour = _palette[i - 1].StoredColour;
        }
        _palette[_paletteSquaresPerRow + 1].StoredColour = colour;
    }
    
    public void RemoveColourFromPalette(int index)
    {
        if (index < _paletteSquaresPerRow) return;
        for (var i = index; i < _palette.Count - 1; i++)
        {
            _palette[i].StoredColour = _palette[i + 1].StoredColour;
        }
        _palette[^1].StoredColour = null;
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        width = Game1.uiViewport.Width / 4;
        float topEdge = _screenCenter.Y - width / 2f - borderWidth * 2f;
        _colourWheel.CenterPoint = new Vector2(_screenCenter.X, topEdge + width / 2f);
        _colourWheel.Width = _colourWheel.Height = width;
        
        int squaresPerRow = _paletteSquaresPerRow;
        int maxWidthForPaletteSquares = width / squaresPerRow;
        
        // The minimum gap between squares should be (width / 48) because I said so. But I also want the gap to be as large as it possibly can be while still fitting 10 squares inside the overall width. So this just keeps increasing the gap until it's so large that all the squares wouldn't be able to fit anymore.
        int gap;
        while (true)
        {
            gap = (width - squaresPerRow * maxWidthForPaletteSquares) / (squaresPerRow - 1);
            if (gap >= _minimumPaletteSquareGap) break;
            maxWidthForPaletteSquares--;
        }

        PickedColourSlider.UpdateBarBounds(new Rectangle(
            x: (int)_screenCenter.X - width / 2,
            y: (int)(_colourWheel.CenterPoint.Y + _colourWheel.Height / 2 + gap * 2),
            width: width,
            height: maxWidthForPaletteSquares
        ));
        
        // If we can't perfectly distribute the squares and gaps, we want to at least center the row of squares instead.
        int startingX = (int)_screenCenter.X - width / 2;
        int offset = (width - (squaresPerRow * maxWidthForPaletteSquares + (squaresPerRow - 1) * gap)) / 2;
        startingX += offset;
        
        for (int i = 0; i < squaresPerRow * 2; i++)
        {
            int x = startingX + i * (maxWidthForPaletteSquares + gap);
            int y = (int)(_colourWheel.CenterPoint.Y + _colourWheel.Height / 2 + gap * 2) + maxWidthForPaletteSquares + gap * 2;
            if (i >= squaresPerRow) // Second row
            {
                x = startingX + (i - squaresPerRow) * (maxWidthForPaletteSquares + gap);
                y += maxWidthForPaletteSquares + gap;
            }
            
            _palette[i].bounds = new Rectangle(x, y, maxWidthForPaletteSquares, maxWidthForPaletteSquares);
            _palette[i].scale = maxWidthForPaletteSquares;
        }
        
        // This'll make the menu fit all our stuff in it, but only just. Nice n cozy size.
        float totalHeight = _colourWheel.Height + gap * 2 + maxWidthForPaletteSquares * 3 + gap * 3;
        height = (int)totalHeight;
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        _colourWheel.Selected = false;
        foreach (var (_, slider) in _sliders)
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
        _colourWheel.Selected = _colourWheel.containsPoint(x, y);
        if (_colourWheel.Selected) return;
        
        foreach (var (_, slider) in _sliders)
        {
            slider.Selected = slider.containsPoint(x, y);
            if (!slider.Selected) continue;
            
            _selectedSlider = slider;
            break;
        }

        for (var i = 0; i < _palette.Count; i++)
        {
            var square = _palette[i];
            if (!square.containsPoint(x, y)) continue;
            
            if (square.IsAddSquare)
            {
                AddColourToPalette(PickedColourHsv);
                Game1.playSound("shwip");
                break;
            }

            if (square is { StoredColour: not null })
            {
                SetColour(HsvColour.FromXnaColor(square.StoredColour.Value));
            } else SetColour(HsvColour.FromXnaColor(Color.White));
            

            Game1.playSound("smallSelect");
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
        for (var i = 0; i < _palette.Count; i++)
        {
            var square = _palette[i];
            if (!square.containsPoint(x, y)) continue;
            
            if (square.IsAddSquare || square.Locked || !square.StoredColour.HasValue) return;
            
            RemoveColourFromPalette(i);
            Game1.playSound("dwoop");
        }
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

        drawLeftMenu(b);
        drawRightMenu(b);
        drawCenterMenu(b);

        drawMouse(b);
    }

    public void drawCenterMenu(SpriteBatch b)
    {
        Game1.DrawBox(
            x: (int)_colourWheel.CenterPoint.X - width / 2 - borderWidth / 2,
            y: (int)_colourWheel.CenterPoint.Y - width / 2 - borderWidth / 2,
            width: width + borderWidth,
            height: height + borderWidth
        );

        _colourWheel.draw(b);
        drawSelectionCircle(b);

        PickedColourSlider.draw(b);
        
        foreach (var square in _palette)
        {
            square.draw(b);
        }
    }

    public void drawLeftMenu(SpriteBatch b)
    {
        Game1.DrawBox(
            x: (int)_colourWheel.CenterPoint.X - width / 2 - borderWidth / 2 - (int)_leftSectionOffset.X,
            y: (int)_colourWheel.CenterPoint.Y - width / 2 + (int)_leftSectionOffset.Y,
            width: width + borderWidth,
            height: height,
            color: Color.WhiteSmoke
        );
    }

    public void drawRightMenu(SpriteBatch b)
    {
        Game1.DrawBox(
            x: (int)_rightSectionCenter.X - width / 2 - borderWidth / 2,
            y: (int)_rightSectionCenter.Y - width / 2,
            width: width + borderWidth,
            height: height
        );
        
        float leftEdge = _rightSectionCenter.X - width / 2f + borderWidth * 1.125f;
        float topEdge = _rightSectionCenter.Y - width / 2f + borderWidth / 4f;
        Vector2 rgbSize = Game1.dialogueFont.MeasureString("RGB");
        float individualWidth = rgbSize.X / 3f;
        Vector2 textScale = new Vector2(
            Math.Min(width / 6f / rgbSize.X, height / 6f / rgbSize.Y),
            Math.Min(width / 6f / rgbSize.X, height / 6f / rgbSize.Y)
        );
        
        b.DrawString(
            spriteFont: Game1.dialogueFont,
            text: "RGB",
            position: new Vector2(leftEdge, topEdge),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: textScale,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );

        b.Draw(
            texture: Game1.staminaRect,
            position: new Vector2(
                leftEdge + rgbSize.X * textScale.X + borderWidth / 3f,
                topEdge + (rgbSize.Y * textScale.Y) / 3f
            ),
            sourceRectangle: null,
            color: Color.Black,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: new Vector2(
                width - (rgbSize.X * textScale.X + borderWidth / 3f) - borderWidth * 1.125f,
                2
            ),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        int sliderHeight = (int)((height - (rgbSize.Y * textScale.Y * 3f) - borderWidth * 2f) / 10f);
        for (var i = 0; i < 3; i++)
        {
            string sliderKey = i switch
            {
                0 => "Red",
                1 => "Green",
                _ => "Blue",
            };
            ColourSlider slider = _sliders[sliderKey];
            slider.UpdateBarBounds(new Rectangle(
                x: (int)leftEdge + (int)(individualWidth * textScale.X * 2f),
                y: (int)(topEdge + rgbSize.Y * textScale.Y) + i * (int)(sliderHeight * 1.25f),
                width: width - (int)(leftEdge - (_rightSectionCenter.X - width / 1.5f)) - borderWidth - (int)(individualWidth * 1.5f),
                height: sliderHeight
            ));
            float charXPosition = leftEdge + individualWidth * textScale.X * 0.75f;
            float charHeight = rgbSize.Y * textScale.Y * 0.75f;
            float charYPosition = topEdge + rgbSize.Y * textScale.Y + i * (sliderHeight * 1.25f) + sliderHeight / 2f - charHeight / 2.25f;
            if (sliderKey is "Green") charXPosition -= 2;
            b.DrawString(
                spriteFont: Game1.dialogueFont,
                text: sliderKey[0] + ":",
                position: new Vector2(charXPosition, charYPosition),
                color: Game1.textColor,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: textScale * 0.75f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            slider.draw(b);
        }
        
        float hsvTopEdge = topEdge + rgbSize.Y * textScale.Y + 3 * (sliderHeight * 1.25f);
        Vector2 hsvString = Game1.dialogueFont.MeasureString("HSV");
        b.DrawString(
            spriteFont: Game1.dialogueFont,
            text: "HSV",
            position: new Vector2(leftEdge, hsvTopEdge),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: textScale,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        b.Draw(
            texture: Game1.staminaRect,
            position: new Vector2(
                leftEdge + hsvString.X * textScale.X + borderWidth / 3f,
                hsvTopEdge + (hsvString.Y * textScale.Y) / 3f
            ),
            sourceRectangle: null,
            color: Color.Black,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: new Vector2(
                width - (hsvString.X * textScale.X + borderWidth / 3f) - borderWidth * 1.125f,
                2
            ),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        for (var i = 0; i < 3; i++)
        {
            // string sliderKey = i switch
            // {
            //     0 => "Hue",
            //     1 => "Saturation",
            //     _ => "Value",
            // };
            // ColourSlider slider = _sliders[sliderKey];
            // slider.UpdateBarBounds(new Rectangle(
            //     x: (int)leftEdge,
            //     y: (int)hsvTopEdge + i * (int)(sliderHeight * 1.25f),
            //     width: width - (int)(leftEdge - (_rightSectionCenter.X - width / 2f)) - borderWidth * 1,
            //     height: sliderHeight
            // ));
            // slider.draw(b);
        }
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
        
        updateSliderColours();
        
        float sineValue = (float)Math.Sin(time.TotalGameTime.TotalSeconds / 10 * Math.PI * 2f);
        _rightSectionOffset.X = (sineValue + 1f) / 2f * width;
        _rightSectionOffset.X = width;
        _leftSectionOffset.X = -_rightSectionOffset.X;
    }

    public void updateSliderColours()
    {
        PickedColourSlider.UpdateColours(PickedColourHsv.ToXnaColor(), PickedColourHsv.ToXnaColor());
        
        _sliders["Red"].UpdateColours(HsvColour.FromXnaColor(new Color(0, (byte)GetGreen(), (byte)GetBlue())).ToXnaColor(), HsvColour.FromXnaColor(new Color(255, (byte)GetGreen(), (byte)GetBlue())).ToXnaColor());
        _sliders["Green"].UpdateColours(HsvColour.FromXnaColor(new Color((byte)GetRed(), 0, (byte)GetBlue())).ToXnaColor(), HsvColour.FromXnaColor(new Color((byte)GetRed(), 255, (byte)GetBlue())).ToXnaColor());
        _sliders["Blue"].UpdateColours(HsvColour.FromXnaColor(new Color((byte)GetRed(), (byte)GetGreen(), 0)).ToXnaColor(), HsvColour.FromXnaColor(new Color((byte)GetRed(), (byte)GetGreen(), 255)).ToXnaColor());
    }

    public override void cleanupBeforeExit()
    {
        base.cleanupBeforeExit();
        Game1.player.modData.Remove("Spiderbuttons.SpiderUI.ColourPickerPalette");
        
        List<Color> paletteColours = new List<Color>();
        for (var i = _paletteSquaresPerRow; i < _palette.Count; i++)
        {
            var square = _palette[i];
            if (square.StoredColour.HasValue)
            {
                paletteColours.Add(square.StoredColour.Value);
            }
        }
        
        Game1.player.modData["Spiderbuttons.SpiderUI.ColourPickerPalette"] = string.Join(" ", paletteColours.Select(c => c.PackedValue));
    }
}