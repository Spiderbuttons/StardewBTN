using System;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
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
        
        PickedColourBackground.UpdateBarBounds(new Rectangle(
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
        
        float buttonScale = (_colourWheel.Width - borderWidth / 2f) / 8f / _toggleAdvancedControls.sourceRect.Width;
        _toggleAdvancedControls.baseScale = _toggleAdvancedControls.scale = buttonScale;
        _togglePreviewBase.baseScale = _togglePreviewBase.scale = buttonScale;
        _togglePreviewIcon.baseScale = _togglePreviewIcon.scale = buttonScale * 0.69f;
        
        Vector2 buttonCenter = new Vector2(
            x: _colourWheel.CenterPoint.X + (_colourWheel.Width + borderWidth) / 2f - borderWidth / 3f,
            y: _colourWheel.CenterPoint.Y - (_colourWheel.Width + borderWidth) / 2f + _toggleAdvancedControls.sourceRect.Height * buttonScale + borderWidth / 3.75f
        );
        _toggleAdvancedControls.bounds = new Rectangle(
            x: (int)buttonCenter.X - (int)(_toggleAdvancedControls.sourceRect.Width * buttonScale),
            y: (int)buttonCenter.Y - (int)(_toggleAdvancedControls.sourceRect.Height * buttonScale),
            width: (int)(_toggleAdvancedControls.sourceRect.Width * buttonScale),
            height: (int)(_toggleAdvancedControls.sourceRect.Height * buttonScale)
        );
        
        buttonCenter = new Vector2(
            x: _colourWheel.CenterPoint.X - (_colourWheel.Width + borderWidth) / 2f + borderWidth / 3f,
            y: _colourWheel.CenterPoint.Y - (_colourWheel.Width + borderWidth) / 2f + _toggleAdvancedControls.sourceRect.Height * buttonScale + borderWidth / 3.75f
        );
        _togglePreviewBase.bounds = new Rectangle(
            x: (int)buttonCenter.X,
            y: (int)buttonCenter.Y - (int)(_togglePreviewBase.sourceRect.Height * buttonScale),
            width: (int)(_togglePreviewBase.sourceRect.Width * buttonScale),
            height: (int)(_togglePreviewBase.sourceRect.Height * buttonScale)
        );
        _togglePreviewIcon.bounds = new Rectangle(
            x: (int)buttonCenter.X + (int)(_togglePreviewBase.sourceRect.Width * buttonScale) / 2 - (int)(_togglePreviewIcon.sourceRect.Width * buttonScale * 0.75f) / 2 + 1,
            y: (int)buttonCenter.Y - (int)(_togglePreviewBase.sourceRect.Height * buttonScale) / 2 - (int)(_togglePreviewIcon.sourceRect.Height * buttonScale * 0.75f) / 2 + 1,
            width: (int)(_togglePreviewBase.sourceRect.Width * buttonScale),
            height: (int)(_togglePreviewBase.sourceRect.Height * buttonScale)
        );

        if (_showingAdvancedControls)
        {
            _rightSectionOffset.X = width;
        } else _rightSectionOffset.X = 0;
        
        // This'll make the menu fit all our stuff in it, but only just. Nice n cozy size.
        float totalHeight = _colourWheel.Height + gap * 2 + maxWidthForPaletteSquares * 3 + gap * 3;
        height = (int)totalHeight;
    }
    
    public override void update(GameTime time)
    {
        base.update(time);
        
        updateSliderColours();
        
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
    }

    public void updateSliderColours()
    {
        PickedColourSlider.UpdateColours(PickedColourHsv.ToXnaColor() * (float)(_alpha / 100), PickedColourHsv.ToXnaColor() * (float)(_alpha / 100));
        
        _sliders["Red"].UpdateColours(HsvColour.FromXnaColor(new Color(0, (byte)GetGreen(), (byte)GetBlue())).ToXnaColor(), HsvColour.FromXnaColor(new Color(255, (byte)GetGreen(), (byte)GetBlue())).ToXnaColor());
        _sliders["Green"].UpdateColours(HsvColour.FromXnaColor(new Color((byte)GetRed(), 0, (byte)GetBlue())).ToXnaColor(), HsvColour.FromXnaColor(new Color((byte)GetRed(), 255, (byte)GetBlue())).ToXnaColor());
        _sliders["Blue"].UpdateColours(HsvColour.FromXnaColor(new Color((byte)GetRed(), (byte)GetGreen(), 0)).ToXnaColor(), HsvColour.FromXnaColor(new Color((byte)GetRed(), (byte)GetGreen(), 255)).ToXnaColor());
        
        _sliders["Hue"].UpdateColours(new HsvColour(_hue, 100, 100).ToXnaColor(), new HsvColour(_hue, 100, 100).ToXnaColor());
        _sliders["Saturation"].UpdateColours(new HsvColour(_hue, 0, _value).ToXnaColor(), new HsvColour(_hue, 100, _value).ToXnaColor());
        _sliders["Value"].UpdateColours(new HsvColour(_hue, _saturation, 0).ToXnaColor(), new HsvColour(_hue, _saturation, 100).ToXnaColor());
        _sliders["Alpha"].UpdateColours(new HsvColour(_hue, _saturation, _value, 0).ToXnaColor(), new HsvColour(_hue, _saturation, _value, 100).ToXnaColor());
    }
}