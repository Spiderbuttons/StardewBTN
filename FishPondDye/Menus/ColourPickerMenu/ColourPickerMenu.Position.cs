using System;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
    #region LeftMenu
    // * LEFT MENU *//
    private Rectangle GetLeftMenuBounds()
    {
        return new Rectangle(
            x: (int)(_leftSectionCenter.X - width / 2f - borderWidth / 2f),
            y: (int)(_leftSectionCenter.Y - width / 2f),
            width: width + borderWidth,
            height: height
        );
    }
    
    private Rectangle GetSafeLeftMenuBounds()
    {
        Rectangle leftMenuBounds = GetLeftMenuBounds();
        return new Rectangle(
            x: (int)(leftMenuBounds.X + borderWidth / 2.75f),
            y: (int)(leftMenuBounds.Y + borderWidth / 3f),
            width: leftMenuBounds.Width - borderWidth * 2,
            height: (int)(leftMenuBounds.Height - borderWidth / 1.6f)
        );
    }
    #endregion
    
    #region CenterMenu
    // * CENTER MENU * //
    private Rectangle GetColourWheelBounds()
    {
        float topEdge = _screenCenter.Y - width / 2f - borderWidth;
        _colourWheel.CenterPoint = new Vector2(_screenCenter.X, topEdge + width / 2f);
        _colourWheel.Width = _colourWheel.Height = width;
        
        return new Rectangle(
            x: (int)(_screenCenter.X - width / 2f),
            y: (int)(topEdge + width / 2f - width / 2f),
            width: width,
            height: width
        );
    }

    private Vector2 GetColourWheelCenter()
    {
        Rectangle colourWheelBounds = GetColourWheelBounds();
        return new Vector2(
            x: colourWheelBounds.X + colourWheelBounds.Width / 2f,
            y: colourWheelBounds.Y + colourWheelBounds.Height / 2f);
    }
    
    private (int size, int gap) GetPaletteSquareSizeAndGap()
    {
        int maxWidthForPaletteSquares = width / _paletteSquaresPerRow;
        
        // The minimum gap between squares should be (width / 72) because I said so. But I also want the gap to be as large as it possibly can be while still fitting _paletteSquaresPerRow squares inside the overall width. So this just keeps increasing the gap until it's so large that all the squares wouldn't be able to fit anymore.
        int gap;
        while (true)
        {
            gap = (width - _paletteSquaresPerRow * maxWidthForPaletteSquares) / (_paletteSquaresPerRow - 1);
            if (gap >= _minimumPaletteSquareGap) break;
            maxWidthForPaletteSquares--;
        }

        return (maxWidthForPaletteSquares, gap);
    }

    private void PositionPaletteSquares()
    {
        (int squareSize, _) = GetPaletteSquareSizeAndGap();
        for (int i = 0; i < _palette.Count; i++)
        {
            _palette[i].bounds = GetPaletteSquareBounds(i);
            _palette[i].scale = squareSize;
        }
    }

    private Rectangle GetPaletteSquareBounds(int paletteIndex)
    {
        (int squareSize, int gap) = GetPaletteSquareSizeAndGap();
        
        // If we can't perfectly distribute the squares and gaps, we want to at least
        // center the row of squares instead. That's what this offset stuff does.
        int startingX = (int)_screenCenter.X - width / 2;
        int offset = (width - (_paletteSquaresPerRow * squareSize + (_paletteSquaresPerRow - 1) * gap)) / 2;
        startingX += offset;
        
        int x = startingX + paletteIndex * (squareSize + gap);
        int y = (int)(_colourWheel.CenterPoint.Y + _colourWheel.Height / 2 + gap * 2) + squareSize + gap * 2;
        if (paletteIndex >= _paletteSquaresPerRow) // Second row
        {
            x = startingX + (paletteIndex - _paletteSquaresPerRow) * (squareSize + gap);
            y += squareSize + gap;
        }
        
        return new Rectangle(x, y, squareSize, squareSize);
    }

    private Rectangle GetPickedColourBounds()
    {
        (int squareSize, int gap) = GetPaletteSquareSizeAndGap();
        return new Rectangle(
            x: (int)_screenCenter.X - width / 2,
            y: (int)(_colourWheel.CenterPoint.Y + _colourWheel.Height / 2 + gap * 2),
            width: width,
            height: squareSize
        );
    }

    private float GetAdvancedButtonScale()
    {
        Rectangle colourWheelBounds = GetColourWheelBounds();
        int sourceRectWidth = _toggleAdvancedControls.sourceRect.Width; // Should be 16. Writing this down in case I change what sourceRect I use.
        return (colourWheelBounds.Width - borderWidth / 2f) / 8f / sourceRectWidth;
    }
    
    private Rectangle GetAdvancedButtonBounds()
    {
        Rectangle colourWheelBounds = GetColourWheelBounds();
        float buttonScale = GetAdvancedButtonScale();
        return new Rectangle(
            x: (int)(colourWheelBounds.X + colourWheelBounds.Width - _toggleAdvancedControls.sourceRect.Width * buttonScale),
            y: (int)(colourWheelBounds.Y + colourWheelBounds.Height - _toggleAdvancedControls.sourceRect.Height * buttonScale),
            width: (int)(_toggleAdvancedControls.sourceRect.Width * buttonScale),
            height: (int)(_toggleAdvancedControls.sourceRect.Height * buttonScale)
        );
    }

    private Rectangle GetPreviewButtonBounds()
    {
        Rectangle colourWheelBounds = GetColourWheelBounds();
        float buttonScale = GetAdvancedButtonScale();
        return new Rectangle(
            x: (int)(colourWheelBounds.X),
            y: (int)(colourWheelBounds.Y + colourWheelBounds.Height - _togglePreviewBase.sourceRect.Height * buttonScale),
            width: (int)(_togglePreviewBase.sourceRect.Width * buttonScale),
            height: (int)(_togglePreviewBase.sourceRect.Height * buttonScale)
        );
    }
    #endregion
    
    #region RightMenu
    // * RIGHT MENU *//
    private Rectangle GetRightMenuBounds()
    {
        // Dunno if I want to have it poking out the side when it's not pulled out or not.
        // Note to self: If I DO want it poking out, just remove the " - borderWidth / 2f" from the x position below.
        return new Rectangle(
            x: (int)(_rightSectionCenter.X - width / 2f - borderWidth / 2f),
            y: (int)(_rightSectionCenter.Y - width / 2f),
            width: width + borderWidth,
            height: height
        );
    }

    private Rectangle GetSafeRightMenuBounds()
    {
        Rectangle rightMenuBounds = GetRightMenuBounds();
        return new Rectangle(
            x: (int)(rightMenuBounds.X + borderWidth * 1.6f),
            y: (int)(rightMenuBounds.Y + borderWidth / 3f),
            width: rightMenuBounds.Width - borderWidth * 2,
            height: (int)(rightMenuBounds.Height - borderWidth / 1.6f)
        );
    }

    private Vector2 GetBaseTextScale()
    {
        Rectangle safeBounds = GetSafeRightMenuBounds();
        Vector2 headerSize = Game1.dialogueFont.MeasureString("RGB");
        return new Vector2(safeBounds.Height / 12f / headerSize.Y);
    }
    
    private int GetSliderHeight()
    {
        Vector2 headerSize = Game1.dialogueFont.MeasureString("RGB");
        return (int)((height - headerSize.Y * GetBaseTextScale().Y * 3f - borderWidth * 2f) / 10f);
    }
    
    private Rectangle GetHeaderBounds(int upperBound, string headerText)
    {
        Rectangle safeBounds = GetSafeRightMenuBounds();
        Vector2 headerSize = Game1.dialogueFont.MeasureString(headerText);
        Vector2 baseTextScale = GetBaseTextScale();
        return new Rectangle(
            x: safeBounds.X,
            y: upperBound,
            width: safeBounds.Width,
            height: (int)(headerSize.Y * baseTextScale.Y)
        );
    }

    private Rectangle GetRgbHeaderBounds()
    {
        return GetHeaderBounds(GetSafeRightMenuBounds().Y, "RGB");
    }

    private Rectangle GetHsvHeaderBounds()
    {
        Vector2 headerSize = Game1.dialogueFont.MeasureString("HSV");
        Vector2 baseTextScale = GetBaseTextScale();
        return GetHeaderBounds(
            upperBound: GetRgbSliderBounds(2).Bottom + (int)(headerSize.Y * baseTextScale.Y * 0.75f / 3f),
            headerText: "HSV"
        );
    }

    private Rectangle GetAlphaHeaderBounds()
    {
        Vector2 headerSize = Game1.dialogueFont.MeasureString("Alpha");
        Vector2 baseTextScale = GetBaseTextScale();
        return GetHeaderBounds(
            upperBound: GetHsvSliderBounds(2).Bottom + (int)(headerSize.Y * baseTextScale.Y * 0.75f / 3f),
            headerText: "Alpha"
        );
    }
    
    private int GetSliderYPosition(int headerBottom, int sliderIndex)
    {
        if (sliderIndex is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(nameof(sliderIndex), "Slider index must be 0, 1, or 2.");
        }
        return (int)(headerBottom + sliderIndex * GetSliderHeight() * 1.25f);
    }
    
    private Rectangle GetSliderBounds(int upperBound, int sliderIndex)
    {
        Vector2 prefixSize = Game1.dialogueFont.MeasureString("G");
        Vector2 textScale = GetBaseTextScale();
        Rectangle safeBounds = GetSafeRightMenuBounds();
        float yOffset = GetSliderYPosition(upperBound, sliderIndex);
        int sliderHeight = GetSliderHeight();
        float indent = prefixSize.X * textScale.X * 2;
        return new Rectangle(
            x: (int)(safeBounds.X + prefixSize.X * textScale.X * 2f),
            y: (int)yOffset,
            width: (int)(safeBounds.Width * 0.7f - indent),
            height: sliderHeight
        );
    }
    
    private Rectangle GetRgbSliderBounds(int sliderIndex)
    {
        return GetSliderBounds(GetRgbHeaderBounds().Bottom, sliderIndex);
    }

    private Rectangle GetHsvSliderBounds(int sliderIndex)
    {
        return GetSliderBounds(GetHsvHeaderBounds().Bottom, sliderIndex);
    }

    private Rectangle GetAlphaSliderBounds()
    {
        return GetSliderBounds(GetAlphaHeaderBounds().Bottom, 0);
    }
    
    private Rectangle GetInputBounds(Rectangle sliderBounds)
    {
        Vector2 prefixSize = Game1.dialogueFont.MeasureString("G");
        Vector2 textScale = GetBaseTextScale();
        float indent = prefixSize.X * textScale.X * 2;
        Rectangle safeBounds = GetSafeRightMenuBounds();
        return new Rectangle(
            x: (int)(sliderBounds.Right + indent / 2f),
            y: sliderBounds.Y,
            width: (int)(safeBounds.Width * 0.3f - indent / 2f),
            height: sliderBounds.Height
        );
    }
    
    private Rectangle GetRgbInputBounds(int sliderIndex)
    {
        return GetInputBounds(GetRgbSliderBounds(sliderIndex));
    }
    
    private Rectangle GetHsvInputBounds(int sliderIndex)
    {
        return GetInputBounds(GetHsvSliderBounds(sliderIndex));
    }
    
    private Rectangle GetAlphaInputBounds()
    {
        return GetInputBounds(GetAlphaSliderBounds());
    }

    private Vector2 GetSliderPrefixOffset(Rectangle sliderBounds, string prefix)
    {
        Vector2 prefixSize = Game1.dialogueFont.MeasureString(prefix);
        Vector2 textScale = GetBaseTextScale();
        float indent = prefixSize.X * textScale.X;
        return new Vector2(
            x: sliderBounds.X - indent,
            y: sliderBounds.Y + prefixSize.Y * textScale.Y * 0.7f / 12f
        );
    }

    private Rectangle GetHexHeaderBounds()
    {
        Rectangle safeBounds = GetSafeRightMenuBounds();
        Vector2 headerSize = Game1.dialogueFont.MeasureString("Hex");
        Vector2 baseTextScale = GetBaseTextScale();
        return new Rectangle(
            x: safeBounds.X,
            y: (int)(safeBounds.Bottom - headerSize.Y * baseTextScale.Y),
            width: safeBounds.Width,
            height: (int)(headerSize.Y * baseTextScale.Y)
        );
    }
    
    private Rectangle GetHexInputBounds()
    {
        Rectangle hexHeaderBounds = GetHexHeaderBounds();
        Vector2 maxHexStringSize = Game1.dialogueFont.MeasureString("#BBBBBB4");
        Vector2 baseTextScale = GetBaseTextScale();
        Vector2 headerSize = Game1.dialogueFont.MeasureString("Hex:");
        return new Rectangle(
            x: hexHeaderBounds.X + (int)(headerSize.X * baseTextScale.X * 1.125f),
            y: hexHeaderBounds.Y,
            width: (int)(maxHexStringSize.X * baseTextScale.X * 1.125f),
            height: hexHeaderBounds.Height
        );
    }

    private Rectangle GetRandomButtonBounds()
    {
        Rectangle safeBounds = GetSafeRightMenuBounds();
        float randomButtonScale = GetRandomButtonScale();
        int widthOfButton = (int)(_randomHexButton.sourceRect.Width * randomButtonScale);
        return new Rectangle(
            x: safeBounds.Right - widthOfButton,
            y: safeBounds.Bottom - widthOfButton,
            width: widthOfButton,
            height: widthOfButton
        );
    }

    private float GetRandomButtonScale()
    {
        Rectangle hexInputBounds = GetHexInputBounds();
        float scaleX = hexInputBounds.Width / (float)_randomHexButton.sourceRect.Width;
        float scaleY = hexInputBounds.Height / (float)_randomHexButton.sourceRect.Height;
        return Math.Min(scaleX, scaleY);
    }
    #endregion
}