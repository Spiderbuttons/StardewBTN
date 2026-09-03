using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace TheInfinityTones.Menus.ColourPickerMenu;

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

    private Rectangle GetPickedColourBounds(int colourIndex = 0)
    {
        (int barHeight, int gap) = (24, 16); // TODO: Fix this.
        int barWidth = width / 3 - gap;
        
        int x = colourIndex switch {
            0 => (int)_screenCenter.X - width / 2,
            1 => (int)_screenCenter.X - barWidth / 2,
            2 => (int)_screenCenter.X + width / 2 - barWidth,
            _ => throw new ArgumentOutOfRangeException(nameof(colourIndex), "Colour index must be 0, 1, or 2.")
        };
        
        Rectangle bounds = new Rectangle(
            x: x,
            y: (int)(_screenCenter.Y + height / 2f) - barHeight - gap,
            width: width / 3 - gap,
            height: barHeight
        );
        
        if (_activeColourIndex == colourIndex)
        {
            bounds.Inflate(4, 4);
        }
        
        return bounds;
    }

    private Rectangle GetColourTextBounds(int colourIndex = 0)
    {
        Rectangle colourBarBounds = GetPickedColourBounds(colourIndex);
        string text = colourIndex switch
        {
            0 => "Tone",
            1 => "Shading",
            2 => "Outline",
            _ => throw new ArgumentOutOfRangeException(nameof(colourIndex), "Colour index must be 0, 1, or 2.")
        };
        Vector2 textSize = Game1.smallFont.MeasureString(text);
        if (colourIndex == _activeColourIndex)
        {
            float scaleMultiplier = (float)(1.0 + 0.05 * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0));
            textSize *= scaleMultiplier;
        }

        return new Rectangle(
            x: (int)(colourBarBounds.X + colourBarBounds.Width / 2f - textSize.X / 2f),
            y: (int)(colourBarBounds.Bottom + 4),
            width: (int)(textSize.X),
            height: (int)(textSize.Y)
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
            x: colourWheelBounds.X,
            y: (int)(colourWheelBounds.Y + colourWheelBounds.Height - _togglePreviewBase.sourceRect.Height * buttonScale),
            width: (int)(_togglePreviewBase.sourceRect.Width * buttonScale),
            height: (int)(_togglePreviewBase.sourceRect.Height * buttonScale)
        );
    }
    
    private Rectangle GetCancelButtonBounds()
    {
        Rectangle colourWheelBounds = GetColourWheelBounds();
        float buttonScale = GetAdvancedButtonScale() / 4f;
        return new Rectangle(
            x: colourWheelBounds.X,
            y: colourWheelBounds.Y,
            width: (int)(_cancelButton.sourceRect.Width * buttonScale),
            height: (int)(_cancelButton.sourceRect.Height * buttonScale)
        );
    }
    
    private Rectangle GetConfirmButtonBounds()
    {
        Rectangle colourWheelBounds = GetColourWheelBounds();
        float buttonScale = GetAdvancedButtonScale() / 4f;
        return new Rectangle(
            x: (int)(colourWheelBounds.X + colourWheelBounds.Width - _confirmButton.sourceRect.Width * buttonScale),
            y: colourWheelBounds.Y,
            width: (int)(_confirmButton.sourceRect.Width * buttonScale),
            height: (int)(_confirmButton.sourceRect.Height * buttonScale)
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

    private Rectangle GetAutoPaletteBounds()
    {
        Rectangle safeBounds = GetSafeRightMenuBounds();
        Rectangle hexInputBounds = GetHexInputBounds();
        int autoPaletteHeight = hexInputBounds.Height;
        return new Rectangle(
            x: safeBounds.X,
            y: (int)(hexInputBounds.Y - autoPaletteHeight * 1.25f),
            width: safeBounds.Width,
            height: autoPaletteHeight
        );
    }

    private Rectangle GetAutoPaletteCheckboxBounds()
    {
        Rectangle autoPaletteBounds = GetAutoPaletteBounds();
        int checkboxSize = autoPaletteBounds.Height;
        float hexHeaderWidth = Game1.dialogueFont.MeasureString("Hex:").X * GetBaseTextScale().X;
        
        return new Rectangle(
            x: autoPaletteBounds.X + (int)(hexHeaderWidth / 4 * 0.7f),
            y: autoPaletteBounds.Y,
            width: checkboxSize,
            height: checkboxSize
        );
    }

    private Rectangle GetDarkSkinToggleBounds()
    {
        // above the auto palette bounds
        Rectangle autoPaletteBounds = GetAutoPaletteBounds();
        int toggleHeight = autoPaletteBounds.Height;
        return new Rectangle(
            x: autoPaletteBounds.X,
            y: (int)(autoPaletteBounds.Y - toggleHeight * 1.25f),
            width: autoPaletteBounds.Width,
            height: toggleHeight
        );
    }

    private Rectangle GetDarkSkinCheckboxBounds()
    {
        Rectangle darkSkinToggleBounds = GetDarkSkinToggleBounds();
        int checkboxSize = darkSkinToggleBounds.Height;
        float headerWidth = Game1.dialogueFont.MeasureString("Hex:").X * GetBaseTextScale().X;
        
        return new Rectangle(
            x: darkSkinToggleBounds.X + (int)(headerWidth / 4 * 0.7f),
            y: darkSkinToggleBounds.Y,
            width: checkboxSize,
            height: checkboxSize
        );
    }

    private float GetAutoPaletteCheckboxScale()
    {
        Rectangle checkboxBounds = GetAutoPaletteCheckboxBounds();
        float scaleX = checkboxBounds.Width / (float)_autoPaletteToggle.sourceRect.Width;
        float scaleY = checkboxBounds.Height / (float)_autoPaletteToggle.sourceRect.Height;
        return Math.Min(scaleX, scaleY);
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