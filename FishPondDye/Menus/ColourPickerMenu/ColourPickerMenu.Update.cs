using System;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        width = Game1.uiViewport.Width / 4;
        
        Rectangle colourWheelBounds = GetColourWheelBounds();
        _colourWheel.CenterPoint = new Vector2(colourWheelBounds.X + colourWheelBounds.Width / 2f, colourWheelBounds.Y + colourWheelBounds.Height / 2f);
        _colourWheel.Width = _colourWheel.Height = colourWheelBounds.Width;
        
        PositionPaletteSquares();

        Rectangle pickedColourBounds = GetPickedColourBounds();
        PickedColourSlider.UpdateBarBounds(pickedColourBounds);
        PickedColourBackground.UpdateBarBounds(pickedColourBounds);

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

        _rightSectionOffset.X = _showingAdvancedControls ? width : 0;
        _leftSectionOffset.X = _showingAdvancedControls ? -width : 0;
        
        // This'll make the menu fit all our stuff in it, but only just. Nice n cozy size.
        (int squareSize, int gap) = GetPaletteSquareSizeAndGap();
        float totalHeight = _colourWheel.Height + gap * 2 + squareSize * 3 + gap * 3;
        height = (int)totalHeight;
        
        UpdateSliderPositions();
        UpdateHexInputPosition();
    }
    
    public override void update(GameTime time)
    {
        base.update(time);
        UpdateComponentIDs();
        
        UpdateSliderColours();
        
        float targetRightOffsetX = _showingAdvancedControls ? width : 0f;
        _rightSectionOffset.X = MathHelper.Lerp(_rightSectionOffset.X, targetRightOffsetX, 0.15f);
        if (Math.Abs(_rightSectionOffset.X - targetRightOffsetX) < 0.5f)
        {
            _rightSectionOffset.X = targetRightOffsetX;
        }
        
        float targetLeftOffsetX = _showingAdvancedControls ? -width : 0f;
        _leftSectionOffset.X = MathHelper.Lerp(_leftSectionOffset.X, targetLeftOffsetX, 0.15f);
        if (Math.Abs(_leftSectionOffset.X - targetLeftOffsetX) < 0.5f)
        {
            _leftSectionOffset.X = targetLeftOffsetX;
        }
        
        if (Math.Abs(_rightSectionOffset.X - targetRightOffsetX) > float.Epsilon)
        {
            UpdateSliderPositions();
            UpdateHexInputPosition();
        }
    }

    public void UpdateComponentIDs()
    {
        Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
        bool isLeft = point.X < 0;
        _colourWheel.downNeighborID = isLeft ? 1 : 2;
        _colourWheel.leftNeighborID = isLeft ? 1 : 2;
    }

    public void UpdateHexInputPosition()
    {
        Rectangle inputBounds = GetHexInputBounds();
        _hexInput.X = inputBounds.X;
        _hexInput.Y = inputBounds.Y;
        _hexInput.Width = inputBounds.Width;
        _hexInput.Height = inputBounds.Height;
        
        _randomHexButton.bounds = GetRandomButtonBounds();
        _randomHexButton.baseScale = _randomHexButton.scale = GetRandomButtonScale();
    }

    private void UpdateSliderPositions()
    {
        for (var i = 0; i < 7; i++)
        {
            string sliderKey = i switch
            {
                0 => "Red",
                1 => "Green",
                2 => "Blue",
                3 => "Hue",
                4 => "Saturation",
                5 => "Value",
                _ => "Alpha",
            };
            ColourSlider slider = _sliders[sliderKey];
            
            Rectangle newBarBounds = i switch
            {
                < 3 => GetRgbSliderBounds(i),
                < 6 => GetHsvSliderBounds(i - 3),
                _ => GetAlphaSliderBounds(),
            };
            slider.UpdateBarBounds(newBarBounds);
            
            Rectangle newInputBounds = i switch
            {
                < 3 => GetRgbInputBounds(i),
                < 6 => GetHsvInputBounds(i - 3),
                _ => GetAlphaInputBounds(),
            };
            slider.UpdateInputBounds(newInputBounds);
            slider.Input.UpdateButtonPositions();
        }
    }

    public void UpdateSliderColours()
    {
        Color pickedColour = PickedColourHsv.ToXnaColor() * (float)(_alpha / 100);
        PickedColourSlider.UpdateColours(pickedColour, pickedColour);
        
        Color minRed = new Color(0, (byte)GetGreen(), (byte)GetBlue());
        Color maxRed = new Color(255, (byte)GetGreen(), (byte)GetBlue());
        Color minGreen = new Color((byte)GetRed(), 0, (byte)GetBlue());
        Color maxGreen = new Color((byte)GetRed(), 255, (byte)GetBlue());
        Color minBlue = new Color((byte)GetRed(), (byte)GetGreen(), 0);
        Color maxBlue = new Color((byte)GetRed(), (byte)GetGreen(), 255);
        _sliders["Red"].UpdateColours(minRed, maxRed);
        _sliders["Green"].UpdateColours(minGreen, maxGreen);
        _sliders["Blue"].UpdateColours(minBlue, maxBlue);
        
        Color minSaturation = new HsvColour(_hue, 0, _value).ToXnaColor();
        Color maxSaturation = new HsvColour(_hue, 100, _value).ToXnaColor();
        Color minValue = new HsvColour(_hue, _saturation, 0).ToXnaColor();
        Color maxValue = new HsvColour(_hue, _saturation, 100).ToXnaColor();
        Color minAlpha = new HsvColour(_hue, _saturation, _value, 0).ToXnaColor();
        Color maxAlpha = new HsvColour(_hue, _saturation, _value, 100).ToXnaColor();
        _sliders["Saturation"].UpdateColours(minSaturation, maxSaturation);
        _sliders["Value"].UpdateColours(minValue, maxValue);
        _sliders["Alpha"].UpdateColours(minAlpha, maxAlpha);
    }
}