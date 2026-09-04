using System;
using System.Linq;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace TheInfinityTones.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        width = Game1.uiViewport.Width / 4;
        
        Rectangle colourWheelBounds = GetColourWheelBounds();
        _colourWheel.CenterPoint = new Vector2(colourWheelBounds.X + colourWheelBounds.Width / 2f, colourWheelBounds.Y + colourWheelBounds.Height / 2f);
        _colourWheel.Width = _colourWheel.Height = colourWheelBounds.Width;
        
        UpdateSelectionCircle();
        
        ToneSlider.UpdateBarBounds(GetPickedColourBounds(0));
        ShadingSlider.UpdateBarBounds(GetPickedColourBounds(1));
        OutlineSlider.UpdateBarBounds(GetPickedColourBounds(2));

        float buttonScale = GetAdvancedButtonScale();
        _toggleAdvancedControls.baseScale = _toggleAdvancedControls.scale = buttonScale;
        _togglePreviewBase.baseScale = _togglePreviewBase.scale = buttonScale;
        _togglePreviewIcon.baseScale = _togglePreviewIcon.scale = buttonScale * 0.69f;
        _toggleAdvancedControls.bounds = GetAdvancedButtonBounds();
        
        Rectangle previewButtonBounds = GetPreviewButtonBounds();
        _togglePreviewBase.bounds = previewButtonBounds;
        
        Rectangle previewIconBounds = new Rectangle(
            x: previewButtonBounds.X + (int)(previewButtonBounds.Width * 0.5f) - (int)(_togglePreviewIcon.sourceRect.Width * buttonScale * 0.75f) / 2 + 1,
            y: previewButtonBounds.Y + (int)(previewButtonBounds.Height * 0.5f) - (int)(_togglePreviewIcon.sourceRect.Height * buttonScale * 0.75f) / 2 + 1,
            width: (int)(_togglePreviewIcon.sourceRect.Width * buttonScale * 0.75f),
            height: (int)(_togglePreviewIcon.sourceRect.Height * buttonScale * 0.75f)
        );
        _togglePreviewIcon.bounds = previewIconBounds;
        
        _cancelButton.baseScale = _cancelButton.scale = buttonScale / 4f;
        _cancelButton.bounds = GetCancelButtonBounds();
        
        _confirmButton.baseScale = _confirmButton.scale = buttonScale / 4f;
        _confirmButton.bounds = GetConfirmButtonBounds();

        _rightSectionOffset.X = _showingAdvancedControls ? width : 0;
        _leftSectionOffset.X = _showingPreview ? -width : 0;
        
        // This'll make the menu fit all our stuff in it, but only just. Nice n cozy size.
        (int squareSize, int gap) = (12, 12);
        float totalHeight = _colourWheel.Height + gap * 2 + squareSize * 3 + gap * 3;
        height = (int)totalHeight;

        UpdateAutoPaletteTogglePosition();
        UpdateDarkSkinTogglePosition();
        UpdateSliderPositions();
        UpdateHexInputPosition();
    }

    private HsvColour GetShadingForTone(HsvColour tone)
    {
        decimal shadingHue = tone.H - 20;
        if (shadingHue < 0) shadingHue += 360;
        decimal saturation = tone.S * 1.125M;
        saturation = Math.Clamp(saturation, 0M, 100M);
        
        return new HsvColour(shadingHue, saturation, tone.V * 0.9M);
    }
    
    private HsvColour GetOutlineForTone(HsvColour tone)
    {
        decimal outlineHue = tone.H - 20;
        if (outlineHue < 0) outlineHue += 360;
        decimal saturation = tone.S * 1M;
        saturation = Math.Clamp(saturation, 0M, 100M);
        
        return new HsvColour(outlineHue, saturation, tone.V * 0.3M);
    }
    
    public override void update(GameTime time)
    {
        base.update(time);
        UpdateComponentIDs();

        if (_autoPalette)
        {
            SetColour(GetShadingForTone(PickedColourHsv), 1);
            SetColour(GetOutlineForTone(PickedColourHsv), 2);
        }
        
        UpdateSliderColours();
        UpdateSelectionCircle();
        
        ToneSlider.UpdateBarBounds(GetPickedColourBounds(0));
        ShadingSlider.UpdateBarBounds(GetPickedColourBounds(1));
        OutlineSlider.UpdateBarBounds(GetPickedColourBounds(2));
        
        float targetRightOffsetX = _showingAdvancedControls ? width : 0f;
        _rightSectionOffset.X = MathHelper.Lerp(_rightSectionOffset.X, targetRightOffsetX, 0.15f);
        if (Math.Abs(_rightSectionOffset.X - targetRightOffsetX) < 0.5f)
        {
            _rightSectionOffset.X = targetRightOffsetX;
        }
        
        float targetLeftOffsetX = _showingPreview ? -width : 0f;
        _leftSectionOffset.X = MathHelper.Lerp(_leftSectionOffset.X, targetLeftOffsetX, 0.15f);
        if (Math.Abs(_leftSectionOffset.X - targetLeftOffsetX) < 0.5f)
        {
            _leftSectionOffset.X = targetLeftOffsetX;
        }
        
        if (Math.Abs(_rightSectionOffset.X - targetRightOffsetX) > float.Epsilon)
        {
            UpdateSliderPositions();
            UpdateHexInputPosition();
            UpdateAutoPaletteTogglePosition();
            UpdateDarkSkinTogglePosition();
        }
        
        if (_colourWheel.Selected && Game1.isGamePadThumbstickInMotion())
        {
            ClampGamePadCursorToColourWheel();
        }
        ColourSlider? selectedSlider = _sliders.Values.FirstOrDefault(s => s.Selected);
        if (selectedSlider is not null && Game1.isGamePadThumbstickInMotion())
        {
            ClampGamePadCursorToSlider(selectedSlider);
        }
    }

    private void UpdateSelectionCircle()
    {
        Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
        Vector2 position = new Vector2(
            x: _colourWheel.CenterPoint.X + point.X * _colourWheel.Width / 2f,
            y: _colourWheel.CenterPoint.Y + point.Y * _colourWheel.Height / 2f
        );
        
        _selectionCircle.setPosition(
            (int)position.X - _selectionCircle.bounds.Width / 2,
            (int)position.Y - _selectionCircle.bounds.Height / 2
        );
    }
    
    private void ClampGamePadCursorToColourWheel()
    {
        GamePadState state = Game1.input.GetGamePadState();
        Vector2 stickMovement = new Vector2(
            x: state.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier,
            y: -state.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier
        );
        Vector2 currentPosition = Game1.getMousePosition().ToVector2();
        Vector2 nextPosition = currentPosition + stickMovement;
        
        if (!_colourWheel.containsPoint((int)nextPosition.X, (int)nextPosition.Y))
        {
            Vector2 direction = nextPosition - _colourWheel.CenterPoint;
            direction.Normalize();
            Vector2 clampedPosition = _colourWheel.CenterPoint + direction * (_colourWheel.Width / 2f);
            Game1.setMousePosition((int)clampedPosition.X, (int)clampedPosition.Y);
        }
    }

    private static void ClampGamePadCursorToSlider(ColourSlider slider)
    {
        GamePadState state = Game1.input.GetGamePadState();
        Point stickMovement = new Point(
            x: (int)(state.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier * 0.175f),
            y: (int)(-state.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier * 0.175f)
        );
        Point currentPosition = Game1.getMousePositionRaw();
        Point nextPosition = currentPosition + stickMovement;
        
        Rectangle rawSliderBounds = new Rectangle(
            x: (int)(slider.Bar.Bounds.X * Game1.options.uiScale),
            y: (int)(slider.Bar.Bounds.Y * Game1.options.uiScale),
            width: (int)(slider.Bar.Bounds.Width * Game1.options.uiScale),
            height: (int)(slider.Bar.Bounds.Height * Game1.options.uiScale)
        );
        
        int grabberHalfWidth = (int)(slider.GetGrabberBounds().Width / 8f * Game1.options.uiScale);
        if (nextPosition.X < rawSliderBounds.Left - grabberHalfWidth || nextPosition.X > rawSliderBounds.Right + grabberHalfWidth)
        {
            int clampedX = Math.Clamp(nextPosition.X, rawSliderBounds.Left - grabberHalfWidth, rawSliderBounds.Right + grabberHalfWidth);
            Game1.setMousePositionRaw(
                x: clampedX,
                y: rawSliderBounds.Center.Y + 1
            );
        } else {
            Game1.setMousePositionRaw(
                x: nextPosition.X,
                y: rawSliderBounds.Center.Y + 1
            );
        }
    }

    private void UpdateComponentIDs()
    {
        Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
        bool selectionCircleIsInBottomQuarter = point.Y > 0.5f;
        bool selectionCircleIsInTopQuarter = point.Y < -0.5f;
        bool selectionCircleIsOnRightSide = point.X > 0.35f;
        switch (point.X)
        {
            case < -0.35f:
            case < 0 when !selectionCircleIsInBottomQuarter:
                _selectionCircle.downNeighborID = CC_TOGGLE_PREVIEW;
                break;
            case > 0.35f:
            case >= 0 when !selectionCircleIsInBottomQuarter:
                _selectionCircle.downNeighborID = CC_TOGGLE_ADVANCED;
                break;
            default:
                _selectionCircle.downNeighborID = _autoPalette ? CC_TONE : CC_SHADING;
                break;
        }
        
        _selectionCircle.upNeighborID = point.X < 0 ? CC_CANCEL : CC_CONFIRM;
        _selectionCircle.leftNeighborID = point.Y > 0 ? CC_TOGGLE_PREVIEW : CC_CANCEL;
        _selectionCircle.rightNeighborID = point.Y > 0 ? CC_TOGGLE_ADVANCED : CC_CONFIRM;
        if (selectionCircleIsOnRightSide) _selectionCircle.rightNeighborID = CC_SLIDERS_START;

        if (selectionCircleIsInBottomQuarter)
        {
            _toggleAdvancedControls.leftNeighborID = CC_SELECTION_CIRCLE;
            _togglePreviewBase.rightNeighborID = CC_SELECTION_CIRCLE;
        } else
        {
            _toggleAdvancedControls.leftNeighborID = CC_TOGGLE_PREVIEW;
            _togglePreviewBase.rightNeighborID = CC_TOGGLE_ADVANCED;
        }

        if (selectionCircleIsInTopQuarter)
        {
            _cancelButton.rightNeighborID = CC_SELECTION_CIRCLE;
            _confirmButton.leftNeighborID = CC_SELECTION_CIRCLE;
        } else 
        {
            _cancelButton.rightNeighborID = CC_CONFIRM;
            _confirmButton.leftNeighborID = CC_CANCEL;
        }

        ToneSlider.rightNeighborID = _autoPalette ? ClickableComponent.ID_ignore : CC_SHADING;
    }

    private void UpdateAutoPaletteTogglePosition()
    {
        Rectangle toggleBounds = GetAutoPaletteCheckboxBounds();
        float toggleScale = GetAutoPaletteCheckboxScale();
        _autoPaletteToggle.bounds = toggleBounds;
        _autoPaletteToggle.baseScale = _autoPaletteToggle.scale = toggleScale;
    }
    
    private void UpdateDarkSkinTogglePosition()
    {
        Rectangle toggleBounds = GetDarkSkinCheckboxBounds();
        float toggleScale = GetAutoPaletteCheckboxScale();
        _darkSkinToggle.bounds = toggleBounds;
        _darkSkinToggle.baseScale = _darkSkinToggle.scale = toggleScale;
    }

    private void UpdateHexInputPosition()
    {
        Rectangle inputBounds = GetHexInputBounds();
        _hexInput.X = inputBounds.X;
        _hexInput.Y = inputBounds.Y;
        _hexInput.Width = inputBounds.Width;
        _hexInput.Height = inputBounds.Height;
        _hexInputCC.bounds = inputBounds;
        
        _randomHexButton.bounds = GetRandomButtonBounds();
        _randomHexButton.baseScale = _randomHexButton.scale = GetRandomButtonScale();
    }

    private void UpdateSliderPositions()
    {
        for (var i = 0; i < 6; i++)
        {
            string sliderKey = i switch
            {
                0 => "Red",
                1 => "Green",
                2 => "Blue",
                3 => "Hue",
                4 => "Saturation",
                _ => "Value",
            };
            ColourSlider slider = _sliders[sliderKey];
            
            Rectangle newBarBounds = i switch
            {
                < 3 => GetRgbSliderBounds(i),
                < 6 => GetHsvSliderBounds(i - 3),
                _ => throw new ArgumentOutOfRangeException()
            };
            slider.UpdateBarBounds(newBarBounds);
            
            Rectangle newInputBounds = i switch
            {
                < 3 => GetRgbInputBounds(i),
                < 6 => GetHsvInputBounds(i - 3),
                _ => throw new ArgumentOutOfRangeException()
            };
            slider.UpdateInputBounds(newInputBounds);
            slider.Input?.UpdateButtonPositions();
        }
    }

    private void UpdateSliderColours()
    {
        Color tone = new HsvColour(_hue[0], _saturation[0], _value[0], _alpha[0]).ToXnaColor();
        ToneSlider.UpdateColours(tone, tone);
        
        Color shading = new HsvColour(_hue[1], _saturation[1], _value[1]).ToXnaColor();
        ShadingSlider.UpdateColours(shading, shading);
        
        Color outline = new HsvColour(_hue[2], _saturation[2], _value[2]).ToXnaColor();
        OutlineSlider.UpdateColours(outline, outline);
        
        Color minRed = new Color(0, (byte)GetGreen(), (byte)GetBlue());
        Color maxRed = new Color(255, (byte)GetGreen(), (byte)GetBlue());
        Color minGreen = new Color((byte)GetRed(), 0, (byte)GetBlue());
        Color maxGreen = new Color((byte)GetRed(), 255, (byte)GetBlue());
        Color minBlue = new Color((byte)GetRed(), (byte)GetGreen(), 0);
        Color maxBlue = new Color((byte)GetRed(), (byte)GetGreen(), 255);
        _sliders["Red"].UpdateColours(minRed, maxRed);
        _sliders["Green"].UpdateColours(minGreen, maxGreen);
        _sliders["Blue"].UpdateColours(minBlue, maxBlue);
        
        Color minSaturation = new HsvColour(_hue[_activeColourIndex], 0, _value[_activeColourIndex]).ToXnaColor();
        Color maxSaturation = new HsvColour(_hue[_activeColourIndex], 100, _value[_activeColourIndex]).ToXnaColor();
        Color minValue = new HsvColour(_hue[_activeColourIndex], _saturation[_activeColourIndex], 0).ToXnaColor();
        Color maxValue = new HsvColour(_hue[_activeColourIndex], _saturation[_activeColourIndex], 100).ToXnaColor();
        _sliders["Saturation"].UpdateColours(minSaturation, maxSaturation);
        _sliders["Value"].UpdateColours(minValue, maxValue);
    }
}