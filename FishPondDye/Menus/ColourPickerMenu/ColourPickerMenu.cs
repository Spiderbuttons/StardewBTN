using System;
using System.Collections.Generic;
using System.Linq;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu;

public sealed partial class ColourPickerMenu : IClickableMenu
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
    
    private ClickableTextureComponent _toggleAdvancedControls = new(
        bounds: new Rectangle(0, 0, 16, 16),
        texture: Game1.mouseCursors2,
        sourceRect: new Rectangle(80, 208, 16, 16),
        scale: 1f
    )
    {
        myID = 2,
        leftNeighborID = 1,
        upNeighborID = 0
    };

    private ClickableTextureComponent _togglePreviewBase = new(
        bounds: new Rectangle(0, 0, 16, 16),
        texture: Game1.mouseCursors2,
        sourceRect: new Rectangle(64, 208, 16, 16),
        scale: 1f
    )
    {
        myID = 1,
        rightNeighborID = 2,
        upNeighborID = 0
    };
    
    private ClickableTextureComponent _togglePreviewIcon = new(
        bounds: new Rectangle(0, 0, 16, 16),
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(80, 0, 13, 13),
        scale: 1f
    );

    private ClickableTextureComponent _randomHexButton = new(
        bounds: new Rectangle(0, 0, 10, 10),
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(381, 361, 10, 10),
        scale: 1f
    );
    
    private Vector2 _screenCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2.25f);

    private Vector2 _leftSectionOffset = Vector2.Zero;
    
    private Vector2 _rightSectionOffset = Vector2.Zero;
    private Vector2 _rightSectionCenter => _colourWheel.CenterPoint + _rightSectionOffset;

    private bool _showingAdvancedControls;

    private ColourWheel _colourWheel;

    private decimal _hue;
    private decimal _saturation;
    private decimal _value = 100M;
    private decimal _alpha = 100M;

    private readonly Dictionary<string, ColourSlider> _sliders = [];
    
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

    private ColourSlider PickedColourBackground = new(
        name: "PickedColourSliderBg",
        getBackingValue: () => -1,
        setBackingValue: _ => { },
        min: 0,
        max: 1,
        bounds: new Rectangle(0, 0, 0, 0),
        colourOne: Color.Transparent,
        colourTwo: Color.Transparent
    ) { IsAlphaBar = true };
    
    private float timeUntilNextSound = 50f;

    private HexInput _hexInput;

    private FishPond? _pond;

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
        )
        {
            myID = 0,
            downNeighborID = 1,
            leftNeighborID = 1,
            rightNeighborID = 2
        };

        List<Color>? savedPalette = null;
        if (Game1.player.modData.TryGetValue("Spiderbuttons.SpiderUI.ColourPickerPalette", out string? paletteString))
        {
            if (!string.IsNullOrWhiteSpace(paletteString)) {
                savedPalette = paletteString.Split(' ')
                    .Select(s => new Color(uint.Parse(s)))
                    .ToList();
            }
        }
        
        for (int i = 0; i < _paletteSquaresPerRow * 2; i++)
        {
            Color? colour = i switch 
            {
                0 => Color.Black,
                1 => Color.White,
                2 => Color.Red,
                3 => new Color(0, 255, 0),
                4 => Color.Blue,
                5 => Color.Cyan,
                6 => Color.Magenta,
                7 => Color.Yellow,
                8 => new Color(255, 128, 0),
                9 => new Color(170, 0, 255),
                10 => Color.Green,
                11 => new Color(255, 0, 170),
                _ => null
            };
            
            if (savedPalette is not null && i - 1 >= _paletteSquaresPerRow)
            {
                int savedIndex = i - 1 - _paletteSquaresPerRow;
                if (savedIndex < savedPalette.Count)
                {
                    colour = savedPalette[savedIndex];
                }
            }
            
            _palette.Add(new PaletteSquare(
                name: $"PaletteSquare{i}",
                bounds: Rectangle.Empty,
                storedColour: colour,
                locked: i < _paletteSquaresPerRow
            )
            {
                IsAddSquare = i == _paletteSquaresPerRow
            });
        }

        _sliders["Red"] = new ColourSlider(
            name: "Red",
            getBackingValue: GetRed,
            setBackingValue: SetRed,
            max: 255
        );
        
        _sliders["Green"] = new ColourSlider(
            name: "Green",
            getBackingValue: GetGreen,
            setBackingValue: SetGreen,
            max: 255
        );
        
        _sliders["Blue"] = new ColourSlider(
            name: "Blue",
            getBackingValue: GetBlue,
            setBackingValue: SetBlue,
            max: 255
        );
        
        _sliders["Hue"] = new ColourSlider(
            name: "Hue",
            getBackingValue: GetHue,
            setBackingValue: SetHue,
            max: 360
        ) { IsHueBar = true };
        
        _sliders["Saturation"] = new ColourSlider(
            name: "Saturation",
            getBackingValue: GetSaturation,
            setBackingValue: SetSaturation
        );
        
        _sliders["Value"] = new ColourSlider(
            name: "Value",
            getBackingValue: GetValue,
            setBackingValue: SetValue
        );
        
        _sliders["Alpha"] = new ColourSlider(
            name: "Alpha",
            getBackingValue: GetAlpha,
            setBackingValue: SetAlpha
        ) { IsAlphaBar = true };
        
        _hexInput = new HexInput(
            font: Game1.dialogueFont,
            textColor: Game1.textColor,
            getColour: () => PickedColourRgb,
            setColour: SetColour
        );
        
        gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
        populateClickableComponentList();
        snapCursorToCurrentSnappedComponent();
    }

    public override void populateClickableComponentList()
    {
        base.populateClickableComponentList();
        
        AssignComponentIds();
        
        allClickableComponents.Add(_colourWheel);
        allClickableComponents.Add(_toggleAdvancedControls);
        allClickableComponents.Add(_togglePreviewBase);
        foreach (var (_, slider) in _sliders)
        {
            allClickableComponents.Add(slider);
        }
    }

    private void AssignComponentIds()
    {
        int sliderId = 0;
        foreach (var (key, slider) in _sliders)
        {
            slider.myID = sliderId;
            sliderId++;
        }
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
    
    public decimal GetHue()
    {
        return _hue;
    }
    
    public decimal GetSaturation()
    {
        return _saturation;
    }
    
    public decimal GetValue()
    {
        return _value;
    }
    
    public decimal GetAlpha()
    {
        return _alpha;
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
    
    public void SetHue(decimal hue)
    {
        _hue = hue;
    }
    
    public void SetSaturation(decimal saturation)
    {
        _saturation = saturation;
    }
    
    public void SetValue(decimal value)
    {
        _value = value;
    }

    public void SetColour(HsvColour hsv)
    {
        _hue = hsv.Hue;
        _saturation = hsv.Saturation;
        _value = hsv.Value;
        _alpha = hsv.Alpha;
    }
    
    public void SetAlpha(decimal alpha)
    {
        _alpha = alpha;
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

    public override bool readyToClose()
    {
        if (_hexInput.Selected) return false;
        return base.readyToClose();
    }
}