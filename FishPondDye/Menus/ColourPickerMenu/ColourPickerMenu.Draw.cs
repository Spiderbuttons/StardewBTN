using System;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
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

        PickedColourBackground.draw(b);
        PickedColourSlider.draw(b);
        
        foreach (var square in _palette)
        {
            square.draw(b);
        }
        
        // This is the "Settings" esque button in the top right that opens the advanced stuff (like the sliders and hex input).
        _toggleAdvancedControls.draw(b);
        // And this one is the one that toggles the preview of whatever it is we're colour picking for.
        _togglePreviewBase.draw(b);
        _togglePreviewIcon.draw(b);
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
        float indentedLeftEdge = leftEdge + individualWidth * textScale.X * 0.75f;
        float charHeight = rgbSize.Y * textScale.Y * 0.75f;
        float barLeftEdge = indentedLeftEdge + (individualWidth * 1.25f) * textScale.X;
        float totalWidthAvailable = width - (barLeftEdge - (_rightSectionCenter.X - width / 2f));
        int barWidth = (int)(totalWidthAvailable * 0.7f);
        for (var i = 0; i < 3; i++)
        {
            string sliderKey = i switch
            {
                0 => "Red",
                1 => "Green",
                _ => "Blue",
            };
            ColourSlider slider = _sliders[sliderKey];
            float charYPosition = topEdge + rgbSize.Y * textScale.Y + i * (sliderHeight * 1.25f) + sliderHeight / 2f - charHeight / 2.25f;
            b.DrawString(
                spriteFont: Game1.dialogueFont,
                text: sliderKey[0] + ":",
                position: new Vector2(indentedLeftEdge, charYPosition),
                color: Game1.textColor,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: textScale * 0.75f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            Rectangle newBarBounds = new Rectangle(
                x: (int)barLeftEdge,
                y: (int)(topEdge + rgbSize.Y * textScale.Y) + i * (int)(sliderHeight * 1.25f),
                width: barWidth,
                height: sliderHeight
            );
            Rectangle newInputBounds = new Rectangle(
                x: (int)(barLeftEdge + barWidth + borderWidth / 2.25f),
                y: (int)(topEdge + rgbSize.Y * textScale.Y) + i * (int)(sliderHeight * 1.25f),
                width: (int)(totalWidthAvailable - barWidth - borderWidth / 2.5f),
                height: sliderHeight
            );
            
            slider.UpdateBarBounds(newBarBounds);
            slider.UpdateInputBounds(newInputBounds);
            
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
            string sliderKey = i switch
            {
                0 => "Hue",
                1 => "Saturation",
                _ => "Value",
            };
            ColourSlider slider = _sliders[sliderKey];
            float charYPosition = hsvTopEdge + hsvString.Y * textScale.Y + i * (sliderHeight * 1.25f) + sliderHeight / 2f - charHeight / 2.25f;
            b.DrawString(
                spriteFont: Game1.dialogueFont,
                text: sliderKey[0] + ":",
                position: new Vector2(indentedLeftEdge, charYPosition),
                color: Game1.textColor,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: textScale * 0.75f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            Rectangle newBarBounds = new Rectangle(
                x: (int)barLeftEdge,
                y: (int)(hsvTopEdge + hsvString.Y * textScale.Y) + i * (int)(sliderHeight * 1.25f),
                width: barWidth,
                height: sliderHeight
            );
            Rectangle newInputBounds = new Rectangle(
                x: (int)(barLeftEdge + barWidth + borderWidth / 2.25f),
                y: (int)(hsvTopEdge + hsvString.Y * textScale.Y) + i * (int)(sliderHeight * 1.25f),
                width: (int)(totalWidthAvailable - barWidth - borderWidth / 2.5f),
                height: sliderHeight
            );
            
            slider.UpdateBarBounds(newBarBounds);
            slider.UpdateInputBounds(newInputBounds);
            
            slider.draw(b);
        }
        
        float alphaTopEdge = hsvTopEdge + hsvString.Y * textScale.Y + 3 * (sliderHeight * 1.25f);
        Vector2 alphaString = Game1.dialogueFont.MeasureString("Alpha");
        b.DrawString(
            spriteFont: Game1.dialogueFont,
            text: "Alpha",
            position: new Vector2(leftEdge, alphaTopEdge),
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
                leftEdge + alphaString.X * textScale.X + borderWidth / 3f,
                alphaTopEdge + (alphaString.Y * textScale.Y) / 3f
            ),
            sourceRectangle: null,
            color: Color.Black,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: new Vector2(
                width - (alphaString.X * textScale.X + borderWidth / 3f) - borderWidth * 1.125f,
                2
            ),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        ColourSlider alphaSlider = _sliders["Alpha"];
        float alphaCharYPosition = alphaTopEdge + alphaString.Y * textScale.Y + sliderHeight / 2f - charHeight / 2.25f;
        b.DrawString(
            spriteFont: Game1.dialogueFont,
            text: "A:",
            position: new Vector2(indentedLeftEdge, alphaCharYPosition),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: textScale * 0.75f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        Rectangle alphaBarBounds = new Rectangle(
            x: (int)barLeftEdge,
            y: (int)(alphaTopEdge + alphaString.Y * textScale.Y),
            width: barWidth,
            height: sliderHeight
        );
        Rectangle alphaInputBounds = new Rectangle(
            x: (int)(barLeftEdge + barWidth + borderWidth / 2.25f),
            y: (int)(alphaTopEdge + alphaString.Y * textScale.Y),
            width: (int)(totalWidthAvailable - barWidth - borderWidth / 2.5f),
            height: sliderHeight
        );
        alphaSlider.UpdateBarBounds(alphaBarBounds);
        alphaSlider.UpdateInputBounds(alphaInputBounds);
        alphaSlider.draw(b);
        
        Vector2 hexString = Game1.dialogueFont.MeasureString("Hex:");
        float hexTopEdge = _rightSectionCenter.Y + height / 2f;
        b.DrawString(
            spriteFont: Game1.dialogueFont,
            text: "Hex:",
            position: new Vector2(leftEdge, hexTopEdge),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: textScale,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        _hexInput.X = (int)(leftEdge + hexString.X * textScale.X + borderWidth / 3f);
        _hexInput.Y = (int)(hexTopEdge);
        _hexInput.Width = (int)(width - (hexString.X * textScale.X + borderWidth / 3f) - borderWidth * 1.125f);
        _hexInput.Height = (int)(hexString.Y * textScale.Y);
        _hexInput.Draw(b);
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
}