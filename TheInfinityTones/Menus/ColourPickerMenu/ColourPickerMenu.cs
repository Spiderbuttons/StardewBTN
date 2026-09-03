using System;
using System.Collections.Generic;
using System.Linq;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace TheInfinityTones.Menus.ColourPickerMenu;

public sealed partial class ColourPickerMenu : IClickableMenu
{
    private const int CC_SELECTION_CIRCLE = 0;
    private const int CC_CANCEL = 1;
    private const int CC_CONFIRM = 2;
    private const int CC_TOGGLE_PREVIEW = 3;
    private const int CC_TOGGLE_ADVANCED = 4;

    private const int CC_TONE = 10;
    private const int CC_SHADING = 11;
    private const int CC_OUTLINE = 12;
    
    private const int CC_SLIDERS_START = 200;
    private const int CC_SLIDERS_INPUT_START = 300;
    
    private const int CC_HEX_INPUT = 400;
    private const int CC_RANDOM = 401;
    private const int CC_AUTO_PALETTE = 402;
    private const int CC_DARK_SKIN = 403;
    
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

    private readonly ClickableTextureComponent _selectionCircle = new(
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
    
    private readonly ClickableTextureComponent _cancelButton = new(
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
    
    private readonly ClickableTextureComponent _confirmButton = new(
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

    private readonly ClickableTextureComponent _togglePreviewBase = new(
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
        downNeighborID = CC_TONE,
        fullyImmutable = true
    };

    private readonly ClickableTextureComponent _togglePreviewIcon = new(
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
    
    private readonly ClickableTextureComponent _toggleAdvancedControls = new(
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
        downNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        fullyImmutable = true
    };

    private readonly ClickableTextureComponent _randomHexButton = new(
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
    
    private static Vector2 _screenCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2.25f);

    private Vector2 _leftSectionOffset = Vector2.Zero;
    private Vector2 _leftSectionCenter => _colourWheel.CenterPoint + _leftSectionOffset;
    
    private Vector2 _rightSectionOffset = Vector2.Zero;
    private Vector2 _rightSectionCenter => _colourWheel.CenterPoint + _rightSectionOffset;

    private bool _showingAdvancedControls;
    private bool _showingPreview;

    private readonly ColourWheel _colourWheel;
    
    public int _activeColourIndex = 0;

    private List<decimal> _hue = [0M, 2M, 327M];
    private List<decimal> _saturation = [100M, 54M, 100M];
    private List<decimal> _value = [100M, 87M, 41M];
    private List<decimal> _alpha = [100M, 100M, 100M];

    private readonly Dictionary<string, ColourSlider> _sliders = [];
    
    public HsvColour PickedColourHsv => new(_hue[_activeColourIndex], _saturation[_activeColourIndex], _value[_activeColourIndex], _alpha[_activeColourIndex]);
    public RgbColour PickedColourRgb => PickedColourHsv.ToRgb();

    private bool _autoPalette;
    private bool _darkSkin;

    private ClickableTextureComponent _autoPaletteToggle = new(
        name: "AutoPaletteToggle",
        bounds: Rectangle.Empty,
        label: null,
        hoverText: null,
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(227, 425, 9, 9),
        scale: 1f
    )
    {
        myID = CC_AUTO_PALETTE,
        leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        rightNeighborID = CC_RANDOM,
        upNeighborID = CC_DARK_SKIN,
        downNeighborID = CC_HEX_INPUT,
        upNeighborImmutable = true,
        downNeighborImmutable = true,
    };

    private ClickableTextureComponent _darkSkinToggle = new(
        name: "DarkSkinToggle",
        bounds: Rectangle.Empty,
        label: null,
        hoverText: null,
        texture: Game1.mouseCursors,
        sourceRect: new Rectangle(227, 425, 9, 9),
        scale: 1f
    )
    {
        myID = CC_DARK_SKIN,
        leftNeighborID = CC_TOGGLE_ADVANCED,
        rightNeighborID = CC_RANDOM,
        downNeighborID = CC_AUTO_PALETTE,
        upNeighborImmutable = true,
        downNeighborImmutable = true,
    };

    // Reusing the ColourSlider because it already draws the border I want.
    private readonly ColourSlider ToneSlider = new(
        name: "PickedColourSlider",
        getBackingValue: null,
        setBackingValue: null,
        min: 0,
        max: 1,
        bounds: new Rectangle(0, 0, 0, 0),
        colourOne: Color.White,
        colourTwo: Color.White
    )
    {
        myID = CC_TONE,
        upNeighborID = CC_TOGGLE_PREVIEW,
        rightNeighborID = CC_SHADING,
        IsHorizontal = true
    };
    
    private readonly ColourSlider ShadingSlider = new(
        name: "PickedColourSlider",
        getBackingValue: null,
        setBackingValue: null,
        min: 0,
        max: 1,
        bounds: new Rectangle(0, 0, 0, 0),
        colourOne: Color.White,
        colourTwo: Color.White
    )
    {
        myID = CC_SHADING,
        upNeighborID = CC_SELECTION_CIRCLE,
        leftNeighborID = CC_TONE,
        rightNeighborID = CC_OUTLINE,
        IsHorizontal = true
    };
    
    private readonly ColourSlider OutlineSlider = new(
        name: "PickedColourSlider",
        getBackingValue: null,
        setBackingValue: null,
        min: 0,
        max: 1,
        bounds: new Rectangle(0, 0, 0, 0),
        colourOne: Color.White,
        colourTwo: Color.White
    )
    {
        myID = CC_OUTLINE,
        upNeighborID = CC_TOGGLE_ADVANCED,
        leftNeighborID = CC_SHADING,
        rightNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        IsHorizontal = true
    };
    
    private float timeUntilNextSound = 50f;

    private readonly HexInput _hexInput;
    private readonly ClickableComponent _hexInputCC = new(bounds: Rectangle.Empty, name: "HexInput")
    {
        myID = CC_HEX_INPUT,
        leftNeighborID = ClickableComponent.CUSTOM_SNAP_BEHAVIOR,
        rightNeighborID = CC_RANDOM,
        upNeighborID = CC_AUTO_PALETTE,
        fullyImmutable = true
    };
    
    private readonly Action<List<RgbColour>>? _onConfirm;
    private readonly Action<List<RgbColour>>? _onCancel;
    private readonly Action<SpriteBatch, Rectangle, List<RgbColour>, bool>? _drawPreview;

    /// <summary>
    /// Opens a menu that allows a player to choose a colour from a standard colour picker.
    /// </summary>
    /// <param name="onConfirm">A callback that is called when the player confirms their colour choice. The chosen colour is passed as an argument.</param>
    /// <param name="onCancel">A callback that is called when the player cancels the colour picker. The colour passed as an argument is whatever colour happens to be selected when the player cancels.</param>
    /// <param name="drawPreview"> A callback that is called to draw a preview of whatever the player is choosing a colour for. The arguments are the sprite batch, the bounds of the preview area, and the currently selected colour.</param>
    public ColourPickerMenu(Action<List<RgbColour>>? onConfirm = null, Action<List<RgbColour>>? onCancel = null, Action<SpriteBatch, Rectangle, List<RgbColour>, bool>? drawPreview = null)
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

        _hexInput = new HexInput(
            font: Game1.dialogueFont,
            textColor: Game1.textColor,
            getColour: () => PickedColourRgb,
            setColour: (colour) => SetColour(colour)
        );

        if (ModEntry.StoredPaletteToggle is true)
        {
            _autoPalette = true;
            _autoPaletteToggle.sourceRect.X = 236;
        }

        if (ModEntry.StoredDarkSkinToggle is true)
        {
            _darkSkin = true;
            _darkSkinToggle.sourceRect.X = 236;
        }
        
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
        
        foreach (var (_, slider) in _sliders)
        {
            allClickableComponents.Add(slider);
            if (slider.Input is null) continue;
            
            allClickableComponents.Add(slider.Input.upButton);
            allClickableComponents.Add(slider.Input.downButton);
        }
        
        allClickableComponents.Add(ToneSlider);
        allClickableComponents.Add(ShadingSlider);
        allClickableComponents.Add(OutlineSlider);
        
        allClickableComponents.Add(_hexInputCC);
        allClickableComponents.Add(_randomHexButton);
        allClickableComponents.Add(_autoPaletteToggle);
        allClickableComponents.Add(_darkSkinToggle);
    }

    private void AssignComponentIds()
    {
        int sliderId = CC_SLIDERS_START;
        int sliderInputId = CC_SLIDERS_INPUT_START;

        for (int i = 0; i < _sliders.Count; i++)
        {
            var slider = _sliders.ElementAt(i).Value;
            slider.myID = sliderId;
            slider.leftNeighborID = i == _sliders.Count - 1 ? CC_TOGGLE_ADVANCED : i == 0 ? CC_CONFIRM : CC_SELECTION_CIRCLE;
            slider.downNeighborID = i < _sliders.Count - 1 ? sliderId + 1 : CC_DARK_SKIN;
            slider.upNeighborID = i > 0 ? sliderId - 1 : ClickableComponent.ID_ignore;
            slider.upNeighborImmutable = true;
            slider.downNeighborImmutable = true;
            slider.rightNeighborImmutable = true;
            
            if (i == _sliders.Count - 1)
            {
                _darkSkinToggle.upNeighborID = sliderId;
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
                
                if (i == _sliders.Count - 1)
                {
                    _randomHexButton.upNeighborID = sliderInputId;
                }
                sliderInputId++;
            }
            sliderId++;
        }
    }
    
    public List<RgbColour> GetPickedColours()
    {
        List<RgbColour> pickedColours = [];
        for (int i = 0; i < _hue.Count; i++)
        {
            HsvColour hsv = new(_hue[i], _saturation[i], _value[i], _alpha[i]);
            pickedColours.Add(hsv.ToRgb());
        }
        return pickedColours;
    }

    public void ShowPreview()
    {
        _showingPreview = true;
    }

    public void HidePreview()
    {
        _showingPreview = false;
    }
    
    public void ShowAdvancedControls()
    {
        _showingAdvancedControls = true;
    }
    
    public void HideAdvancedControls()
    {
        _showingAdvancedControls = false;
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
        return _hue[_activeColourIndex];
    }
    
    public decimal GetSaturation()
    {
        return _saturation[_activeColourIndex];
    }
    
    public decimal GetValue()
    {
        return _value[_activeColourIndex];
    }
    
    public decimal GetAlpha()
    {
        return _alpha[_activeColourIndex];
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

    public void SetColour(RgbColour rgb, int colourIndex = -1)
    {
        HsvColour hsv = rgb.ToHsv();
        SetColour(hsv, colourIndex);
    }
    
    public void SetHue(decimal hue)
    {
        _hue[_activeColourIndex] = hue;
    }
    
    public void SetSaturation(decimal saturation)
    {
        _saturation[_activeColourIndex] = saturation;
    }
    
    public void SetValue(decimal value)
    {
        _value[_activeColourIndex] = value;
    }

    public void SetColour(HsvColour hsv, int colourIndex = -1)
    {
        if (colourIndex is -1) colourIndex = _activeColourIndex;
        _hue[colourIndex] = hsv.Hue;
        _saturation[colourIndex] = hsv.Saturation;
        _value[colourIndex] = hsv.Value;
        _alpha[colourIndex] = hsv.Alpha;
    }
    
    public void SetAlpha(decimal alpha)
    {
        _alpha[_activeColourIndex] = alpha;
    }

    public override void cleanupBeforeExit()
    {
        base.cleanupBeforeExit();
    }

    public override bool readyToClose()
    {
        if (_hexInput.Selected) return false;
        return base.readyToClose();
    }
}