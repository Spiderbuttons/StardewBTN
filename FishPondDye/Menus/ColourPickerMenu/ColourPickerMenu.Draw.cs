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
        Rectangle rightMenuBounds = GetRightMenuBounds();
        Game1.DrawBox(
            x: rightMenuBounds.X,
            y: rightMenuBounds.Y,
            width: rightMenuBounds.Width,
            height: rightMenuBounds.Height
        );
        
        drawRightHeaders(b);
        drawRightSliders(b);
        drawRightHexInput(b);
    }

    private void drawRightSliders(SpriteBatch b)
    {
        foreach (var (key, slider) in _sliders)
        {
            b.DrawString(
                spriteFont: Game1.dialogueFont,
                text: key[0] + ":",
                position: GetSliderPrefixOffset(slider.Bar.Bounds, key[0] + ":"),
                color: Game1.textColor,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: GetBaseTextScale() * 0.75f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            slider.draw(b);
        }
    }

    private void drawRightHexInput(SpriteBatch b)
    {
        Rectangle hexHeaderBounds = GetHexHeaderBounds();
        drawHeader(b, "Hex:", hexHeaderBounds, includeLine: false);
        
        _hexInput.Draw(b);
        _randomHexButton.draw(b);
    }

    private void drawRightHeaders(SpriteBatch b)
    {
        Rectangle rgbHeaderBounds = GetRgbHeaderBounds();
        drawHeader(b, "RGB", rgbHeaderBounds);
        
        Rectangle hsvHeaderBounds = GetHsvHeaderBounds();
        drawHeader(b, "HSV", hsvHeaderBounds);
        
        Rectangle alphaHeaderBounds = GetAlphaHeaderBounds();
        drawHeader(b, "Alpha", alphaHeaderBounds);
    }

    private void drawHeader(SpriteBatch b, string header, Rectangle bounds, bool includeLine = true)
    {
        Rectangle safeBounds = GetSafeRightMenuBounds();
        Vector2 textScale = GetBaseTextScale();
        Vector2 headerString = Game1.dialogueFont.MeasureString(header);
        
        b.DrawString(
            spriteFont: Game1.dialogueFont,
            text: header,
            position: new Vector2(bounds.X, bounds.Y),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: textScale,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        if (!includeLine) return;
        
        b.Draw(
            texture: Game1.staminaRect,
            position: new Vector2(
                x: bounds.X + headerString.X * textScale.X * 1.125f,
                y: bounds.Y + headerString.Y * textScale.Y / 3f
            ),
            sourceRectangle: null,
            color: Color.Black,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: new Vector2(
                x: safeBounds.Width - headerString.X * textScale.X * 1.125f,
                y: 2
            ),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
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