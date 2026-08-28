using System;
using System.Collections.Generic;
using System.Linq;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu;

public sealed partial class ColourPickerMenu : IClickableMenu
{
    private const int CC_SELECTION_CIRCLE = 0;
    private const int CC_CANCEL = 1;
    private const int CC_CONFIRM = 2;
    private const int CC_TOGGLE_PREVIEW = 3;
    private const int CC_TOGGLE_ADVANCED = 4;

    private const int CC_PALETTE_START = 100;
    private const int CC_SLIDERS_START = 200;
    private const int CC_SLIDERS_INPUT_START = 300;
    
    private const int CC_HEX_INPUT = 400;
    private const int CC_RANDOM = 401;
    
    private static Texture2D _selectionCircleTexture
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

    private ClickableTextureComponent _selectionCircle = new(
        bounds: new Rectangle(0, 0, 18, 18),
        texture: _selectionCircleTexture,
        sourceRect: new Rectangle(0, 0, 9, 9),
        scale: 2f
    )
    {
        name = "SelectionCircle",
        myID = CC_SELECTION_CIRCLE,
        downNeighborID = CC_TOGGLE_PREVIEW,
        leftNeighborID = CC_TOGGLE_PREVIEW,
        rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        upNeighborID = CC_CONFIRM,
        upNeighborImmutable = true,
        leftNeighborImmutable = true,
        rightNeighborImmutable = true,
    };
    
    private ClickableTextureComponent _cancelButton = new(
        bounds: new Rectangle(0, 0, 64, 64),
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(192, 256, 64, 64),
        scale: 1f / 4f
    )
    {
        name = "CancelButton",
        myID = CC_CANCEL,
        downNeighborID = CC_SELECTION_CIRCLE,
        rightNeighborID = CC_CONFIRM,
        fullyImmutable = true
    };
    
    private ClickableTextureComponent _confirmButton = new(
        bounds: new Rectangle(0, 0, 64, 64),
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(128, 256, 64, 64),
        scale: 1f / 4f
    )
    {
        name = "ConfirmButton",
        myID = CC_CONFIRM,
        downNeighborID = CC_SELECTION_CIRCLE,
        rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        leftNeighborID = CC_CANCEL,
        fullyImmutable = true
    };

    private ClickableTextureComponent _togglePreviewBase = new(
        bounds: new Rectangle(0, 0, 16, 16),
        texture: Game1.mouseCursors2,
        sourceRect: new Rectangle(64, 208, 16, 16),
        scale: 1f
    )
    {
        name = "TogglePreviewBase",
        hoverText = "Preview",
        myID = CC_TOGGLE_PREVIEW,
        rightNeighborID = CC_TOGGLE_ADVANCED,
        upNeighborID = CC_SELECTION_CIRCLE,
        downNeighborID = CC_PALETTE_START,
        fullyImmutable = true
    };

    private ClickableTextureComponent _togglePreviewIcon = new(
        bounds: new Rectangle(0, 0, 16, 16),
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(80, 0, 13, 13),
        scale: 1f
    )
    {
        name = "TogglePreviewIcon",
        myID = ClickableComponent.ID_ignore,
        fullyImmutable = true
    };
    
    private ClickableTextureComponent _toggleAdvancedControls = new(
        bounds: new Rectangle(0, 0, 16, 16),
        texture: Game1.mouseCursors2,
        sourceRect: new Rectangle(80, 208, 16, 16),
        scale: 1f
    )
    {
        name = "ToggleAdvancedControls",
        myID = CC_TOGGLE_ADVANCED,
        leftNeighborID = CC_TOGGLE_PREVIEW,
        upNeighborID = CC_SELECTION_CIRCLE,
        downNeighborID = CC_PALETTE_START + _paletteSquaresPerRow - 1,
        rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        fullyImmutable = true
    };

    private ClickableTextureComponent _randomHexButton = new(
        bounds: new Rectangle(0, 0, 10, 10),
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(381, 361, 10, 10),
        scale: 1f
    )
    {
        name = "RandomHexButton",
        myID = CC_RANDOM,
        leftNeighborID = CC_HEX_INPUT,
        fullyImmutable = true
    };
    
    private Vector2 _screenCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2.25f);

    private Vector2 _leftSectionOffset = Vector2.Zero;
    private Vector2 _leftSectionCenter => _colourWheel.CenterPoint + _leftSectionOffset;
    
    private Vector2 _rightSectionOffset = Vector2.Zero;
    private Vector2 _rightSectionCenter => _colourWheel.CenterPoint + _rightSectionOffset;

    private bool _showingAdvancedControls;
    private bool _showingPreview;

    private ColourWheel _colourWheel;

    private decimal _hue;
    private decimal _saturation;
    private decimal _value = 100M;
    private decimal _alpha = 100M;

    private readonly Dictionary<string, ColourSlider> _sliders = [];
    
    private const int _paletteSquaresPerRow = 12;
    private int _minimumPaletteSquareGap => width / 72;
    private List<PaletteSquare> _palette = [];
    
    public HsvColour PickedColourHsv => new(_hue, _saturation, _value, _alpha);
    public RgbColour PickedColourRgb => PickedColourHsv.ToRgb();

    // Reusing the ColourSlider because it already draws the border I want.
    private ColourSlider PickedColourSlider = new(
        name: "PickedColourSlider",
        getBackingValue: null,
        setBackingValue: null,
        min: 0,
        max: 1,
        bounds: new Rectangle(0, 0, 0, 0),
        colourOne: Color.White,
        colourTwo: Color.White
    ) { IsHorizontal = true };
    
    private float timeUntilNextSound = 50f;

    private HexInput _hexInput;
    private ClickableComponent _hexInputCC = new(bounds: Rectangle.Empty, name: "HexInput")
    {
        myID = CC_HEX_INPUT,
        leftNeighborID = CC_TOGGLE_ADVANCED,
        rightNeighborID = CC_RANDOM,
        fullyImmutable = true
    };
    
    private readonly Action<RgbColour>? _onConfirm;
    private readonly Action<RgbColour>? _onCancel;
    private readonly Action<SpriteBatch, Rectangle, RgbColour>? _drawPreview;

    /// <summary>
    /// Opens a menu that allows a player to choose a colour from a standard colour picker.
    /// </summary>
    /// <param name="onConfirm">A callback that is called when the player confirms their colour choice. The chosen colour is passed as an argument.</param>
    /// <param name="onCancel">A callback that is called when the player cancels the colour picker. The colour passed as an argument is whatever colour happens to be selected when the player cancels.</param>
    /// <param name="drawPreview"> A callback that is called to draw a preview of whatever the player is choosing a colour for. The arguments are the sprite batch, the bounds of the preview area, and the currently selected colour.</param>
    public ColourPickerMenu(Action<RgbColour>? onConfirm = null, Action<RgbColour>? onCancel = null, Action<SpriteBatch, Rectangle, RgbColour>? drawPreview = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        _drawPreview = drawPreview;
        
        width = Game1.uiViewport.Width / 4;
        height = Game1.uiViewport.Width / 4;

        _colourWheel = new ColourWheel(
            name: "ColourWheel",
            centerPoint: new Vector2(_screenCenter.X, _screenCenter.Y - height / 6f),
            width: width,
            height: width
        );

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
        )
        {
            IsHueBar = true
        };

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
        )
        {
            IsAlphaBar = true
        };

        _hexInput = new HexInput(
            font: Game1.dialogueFont,
            textColor: Game1.textColor,
            getColour: () => PickedColourRgb,
            setColour: SetColour
        );
        
        gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
        populateClickableComponentList();
        setCurrentlySnappedComponentTo(CC_SELECTION_CIRCLE);
        if (Game1.options.SnappyMenus) snapCursorToCurrentSnappedComponent();
    }

    public override void populateClickableComponentList()
    {
        if (allClickableComponents is null) allClickableComponents = [];
        else allClickableComponents.Clear();
        
        AssignComponentIds();
        
        allClickableComponents.Add(_selectionCircle);
        allClickableComponents.Add(_toggleAdvancedControls);
        allClickableComponents.Add(_togglePreviewBase);
        allClickableComponents.Add(_cancelButton);
        allClickableComponents.Add(_confirmButton);
        
        foreach (var square in _palette)
        {
            allClickableComponents.Add(square);
        }
        
        foreach (var (_, slider) in _sliders)
        {
            allClickableComponents.Add(slider);
            if (slider.Input is not null)
            {
                allClickableComponents.Add(slider.Input.upButton);
                allClickableComponents.Add(slider.Input.downButton);
            }
        }
        allClickableComponents.Add(_hexInputCC);
        allClickableComponents.Add(_randomHexButton);
    }

    private void AssignComponentIds()
    {
        for (int i = 0; i < _palette.Count; i++)
        {
            var square = _palette[i];
            square.myID = CC_PALETTE_START + i;
            square.upNeighborID = i < _paletteSquaresPerRow ? CC_SELECTION_CIRCLE : CC_PALETTE_START + i - _paletteSquaresPerRow;
            if (i is < _paletteSquaresPerRow and (< 2 or >= _paletteSquaresPerRow - 2))
            {
                square.upNeighborID = i < 4 ? CC_TOGGLE_PREVIEW : CC_TOGGLE_ADVANCED;
            }
            square.downNeighborID = i < _paletteSquaresPerRow ? CC_PALETTE_START + i + _paletteSquaresPerRow : ClickableComponent.ID_ignore;
            square.rightNeighborID = (i + 1) % _paletteSquaresPerRow == 0 ? ClickableComponent.ID_ignore : CC_PALETTE_START + i + 1;
            square.leftNeighborID = i % _paletteSquaresPerRow == 0 ? ClickableComponent.ID_ignore : CC_PALETTE_START + i - 1;
            square.fullyImmutable = true;
        }
        
        int sliderId = CC_SLIDERS_START;
        int sliderInputId = CC_SLIDERS_INPUT_START;

        for (int i = 0; i < _sliders.Count; i++)
        {
            var slider = _sliders.ElementAt(i).Value;
            slider.myID = sliderId;
            slider.leftNeighborID = i == _sliders.Count - 1 ? CC_TOGGLE_ADVANCED : i == 0 ? CC_CONFIRM : CC_SELECTION_CIRCLE;
            slider.downNeighborID = i < _sliders.Count - 1 ? sliderId + 1 : CC_HEX_INPUT;
            slider.upNeighborID = i > 0 ? sliderId - 1 : ClickableComponent.ID_ignore;
            slider.upNeighborImmutable = true;
            slider.downNeighborImmutable = true;
            slider.rightNeighborImmutable = true;
            
            if (i == _sliders.Count - 1)
            {
                _hexInputCC.upNeighborID = sliderId;
            }
            
            var input = slider.Input;
            if (input is not null)
            {
                slider.rightNeighborID = sliderInputId;
                
                var upButton = input.upButton;
                var downButton = input.downButton;
                
                upButton.myID = sliderInputId;
                upButton.downNeighborID = sliderInputId + 1;
                upButton.upNeighborID = i > 0 ? sliderInputId - 1 : ClickableComponent.ID_ignore;
                upButton.fullyImmutable = true;
                upButton.leftNeighborID = sliderId;
                sliderInputId++;
                
                downButton.myID = sliderInputId;
                downButton.upNeighborID = sliderInputId - 1;
                downButton.downNeighborID = i < _sliders.Count - 1 ? sliderInputId + 1 : CC_RANDOM;
                downButton.fullyImmutable = true;
                downButton.leftNeighborID = sliderId;
                sliderInputId++;
            }
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