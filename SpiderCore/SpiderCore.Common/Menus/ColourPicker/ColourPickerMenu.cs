using System;
using System.Collections.Generic;
using System.Linq;
using SpiderCore.Common.Colour;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpiderCore.Common.Menus.ColourPicker
{
    public partial class ColourPickerMenu : IClickableMenu
    {
        public enum CloseReason
        {
            Confirmed,
            Cancelled,
            Other
        }
        
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
            downNeighborID = CC_PALETTE_START,
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

        private decimal _hue;
        private decimal _saturation;
        private decimal _value = 100M;
        private decimal _alpha = 100M;

        private readonly Dictionary<string, ColourSlider> _sliders = [];
    
        private readonly int _paletteSquaresPerRow;
        private int _paletteRows;
        private int _minimumPaletteSquareGap => width / 72;
        private readonly List<PaletteSquare> _palette = [];
    
        public HsvColour PickedColourHsv => new(_hue, _saturation, _value, _alpha);
        public RgbColour PickedColourRgb => PickedColourHsv.ToRgb();

        // Reusing the ColourSlider because it already draws the border I want.
        private readonly ColourSlider PickedColourSlider = new(
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

        private readonly HexInput _hexInput;
        private readonly ClickableComponent _hexInputCC = new(bounds: Rectangle.Empty, name: "HexInput")
        {
            myID = CC_HEX_INPUT,
            leftNeighborID = CC_TOGGLE_ADVANCED,
            rightNeighborID = CC_RANDOM,
            fullyImmutable = true
        };
        
        private Action<CloseReason, RgbColour, object?>? _onClose;
        private readonly Action<SpriteBatch, Rectangle, RgbColour, object?>? _drawPreview;
        private readonly bool _allowAlpha;

        private readonly object? _previewObject;
        private readonly object? _colourableObject;
        private readonly IClickableMenu? _previousMenu;

        /// <summary>
        /// Opens a menu that allows a player to choose a colour from a standard colour picker.
        /// </summary>
        /// <param name="onClose">A callback that is invoked when the menu is closed. The chosen colour is passed as an argument, as well as an enum to determine whether the menu was closed because the player confirmed their choice, cancelled picking a colour, or the menu was closed due to an emergency shutdown.</param>
        /// <param name="drawPreview">A callback that is called to draw a preview of whatever the player is choosing a colour for. The arguments are the sprite batch, the bounds of the preview area, the currently selected colour, and whatever preview object you provided to the menu, if anything.</param>
        /// <param name="previewObject">An object that is passed to the drawPreview callback. This can be used to pass in whatever the player is choosing a colour for, so that it can be drawn in the preview area.</param>
        /// <param name="colourableObject">An object that is passed to the onClose callback. This can be used to pass in whatever the player is choosing a colour for, so that it can be used when the player confirms their choice.</param>
        /// <param name="previousMenu">The menu that was open before the colour picker was opened. This menu will be drawn behind the colour picker, and will be returned to when the colour picker is closed.</param>
        /// <param name="allowAlpha">Whether the player is allowed to choose an alpha value for their colour. If false, the alpha will always be 100.</param>
        /// <param name="paletteSquaresPerRow">The number of palette squares to put in a single row.</param>
        /// <param name="paletteRows">The number of rows of palette squares to display.</param>
        /// <param name="paletteColours">A list of colours to use for the palette. If null, a default palette will be used. Any palette colours that would be placed in the last row will be omitted in order to leave room for the player's saved palette.</param>
        public ColourPickerMenu(Action<CloseReason, RgbColour, object?>? onClose = null, Action<SpriteBatch, Rectangle, RgbColour, object?>? drawPreview = null, object? previewObject = null, object? colourableObject = null, IClickableMenu? previousMenu = null, bool allowAlpha = true, int paletteSquaresPerRow = 12, int paletteRows = 2, IEnumerable<Color>? paletteColours = null)
        {
            _previousMenu = previousMenu;
            exitFunction = () => Game1.activeClickableMenu = _previousMenu;
            
            _onClose = onClose;
            _drawPreview = drawPreview;
            _previewObject = previewObject;
            _colourableObject = colourableObject;
            _allowAlpha = allowAlpha;

            width = (int)(Game1.uiViewport.Width / 4f);
            height = width;
            
            _colourWheel = new ColourWheel(
                name: "ColourWheel",
                centerPoint: new Vector2(_screenCenter.X, _screenCenter.Y - height / 6f),
                width: width,
                height: width
            );
            
            _paletteSquaresPerRow = paletteSquaresPerRow;
            _paletteRows = paletteRows;
            _toggleAdvancedControls.downNeighborID = CC_PALETTE_START + _paletteSquaresPerRow - 1;

            List<Color>? savedPalette = null;
            // Do not change this key!! Keeping this key the same ensures that a player's palette will be saved and loaded
            // regardless of which mod is using this colour picker menu!
            if (Game1.player.modData.TryGetValue("Spiderbuttons.SpiderUI.ColourPickerPalette", out string? paletteString))
            {
                if (!string.IsNullOrWhiteSpace(paletteString)) {
                    savedPalette = paletteString.Split(' ')
                        .Select(s => new Color(uint.Parse(s)))
                        .ToList();
                }
            }
            
            List<Color> paletteColoursList = paletteColours?.ToList() ?? [
                Color.Black,
                Color.White,
                Color.Red,
                new Color(0, 255, 0),
                Color.Blue,
                Color.Cyan,
                Color.Magenta,
                Color.Yellow,
                new Color(255, 128, 0),
                new Color(170, 0, 255),
                Color.Green,
                new Color(255, 0, 170)
            ];
        
            for (int i = 0; i < _paletteSquaresPerRow * _paletteRows; i++)
            {
                Color? colour = null;
                if (i < paletteColoursList.Count && i < _paletteSquaresPerRow * (_paletteRows - 1))
                {
                    colour = paletteColoursList[i];
                }
                
                if (savedPalette is not null && i - 1 >= _paletteSquaresPerRow * (paletteRows - 1))
                {
                    int savedIndex = i - 1 - _paletteSquaresPerRow * (paletteRows - 1);
                    if (savedIndex < savedPalette.Count)
                    {
                        colour = savedPalette[savedIndex];
                    }
                }
            
                _palette.Add(new PaletteSquare(
                    name: $"PaletteSquare{i}",
                    bounds: Rectangle.Empty,
                    storedColour: colour,
                    locked: i < _paletteSquaresPerRow * (paletteRows - 1)
                )
                {
                    IsAddSquare = i == _paletteSquaresPerRow * (paletteRows - 1),
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

            if (_allowAlpha)
            {
                _sliders["Alpha"] = new ColourSlider(
                    name: "Alpha",
                    getBackingValue: GetAlpha,
                    setBackingValue: SetAlpha
                )
                {
                    IsAlphaBar = true
                };
            }

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
                if (slider.Input is null) continue;
            
                allClickableComponents.Add(slider.Input.upButton);
                allClickableComponents.Add(slider.Input.downButton);
            }
            allClickableComponents.Add(_hexInputCC);
            allClickableComponents.Add(_randomHexButton);
        }

        private void AssignComponentIds()
        {
            for (int i = 0; i < _palette.Count; i++)
            {
                int indexInRow = i % _paletteSquaresPerRow;
                var square = _palette[i];
                square.myID = CC_PALETTE_START + i;
                square.upNeighborID = i < _paletteSquaresPerRow ? CC_SELECTION_CIRCLE : CC_PALETTE_START + i - _paletteSquaresPerRow;
                // TODO: Fix.
                if (i < _paletteSquaresPerRow && (indexInRow < 2 || indexInRow >= _paletteSquaresPerRow - 2))
                {
                    square.upNeighborID = i < 2 ? CC_TOGGLE_PREVIEW : CC_TOGGLE_ADVANCED;
                }
                square.downNeighborID = i + _paletteSquaresPerRow < _palette.Count ? CC_PALETTE_START + i + _paletteSquaresPerRow : ClickableComponent.ID_ignore;
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
            RgbColour rgb = new RgbColour(red, GetGreen(), GetBlue(), GetAlpha() / 100M * 255M);
            SetColour(rgb);
        }
    
        public void SetGreen(decimal green)
        {
            RgbColour rgb = new RgbColour(GetRed(), green, GetBlue(), GetAlpha() / 100M * 255M);
            SetColour(rgb);
        }
    
        public void SetBlue(decimal blue)
        {
            RgbColour rgb = new RgbColour(GetRed(), GetGreen(), blue, GetAlpha() / 100M * 255M);
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
    
        private void AddColourToPalette(HsvColour hsv)
        {
            Color colour = hsv.ToXnaColor();
            for (var i = _palette.Count - 1; i > _paletteSquaresPerRow * (_paletteRows - 1); i--)
            {
                _palette[i].StoredColour = _palette[i - 1].StoredColour;
            }
            _palette[_paletteSquaresPerRow * (_paletteRows - 1) + 1].StoredColour = colour;
        }
    
        private void RemoveColourFromPalette(int index)
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
            // Do not change this key!! Keeping this key the same ensures that a player's palette will be saved and loaded
            // regardless of which mod is using this colour picker menu!
            Game1.player.modData.Remove("Spiderbuttons.SpiderUI.ColourPickerPalette");
        
            List<Color> paletteColours = [];
            for (var i = _paletteSquaresPerRow * (_paletteRows - 1); i < _palette.Count; i++)
            {
                var square = _palette[i];
                if (square.StoredColour.HasValue)
                {
                    paletteColours.Add(square.StoredColour.Value);
                }
            }
        
            // Do not change this key!! Keeping this key the same ensures that a player's palette will be saved and loaded
            // regardless of which mod is using this colour picker menu!
            Game1.player.modData["Spiderbuttons.SpiderUI.ColourPickerPalette"] = string.Join(" ", paletteColours.Select(c => c.PackedValue));
            
            // In the input code, we null this callback out after clicking confirm or cancel.
            // So if it's still here by this point, then the menu must be closing for some other reason.
            _onClose?.Invoke(CloseReason.Other, PickedColourRgb, _colourableObject);
        }

        public override bool readyToClose()
        {
            if (_hexInput.Selected) return false;
            return base.readyToClose();
        }

        public override void emergencyShutDown()
        {
            _onClose?.Invoke(CloseReason.Other, PickedColourRgb, _colourableObject);
            base.emergencyShutDown();
        }
    }
}