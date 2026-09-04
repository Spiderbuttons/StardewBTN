using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using TheInfinityTones.Helpers;

namespace TheInfinityTones.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
    public override void draw(SpriteBatch b)
    {
        if (ModEntry.GetStoredCustomizationMenu() is { } menu)
        {
            if (b.GraphicsDevice.GetRenderTargets().FirstOrDefault().RenderTarget is not RenderTarget2D target) goto afterMenu;

            menu.draw(b);
            b.End();
            
            ModEntry.BlurEffect.Parameters["BlurRadius"].SetValue(5f);
            ModEntry.BlurEffect.Parameters["Resolution"].SetValue(new Vector2(target.Width, target.Height));
            
            b.BeginWithShader(shader: ModEntry.BlurEffect);
            b.Draw(
                texture: target,
                position: Vector2.Zero,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: 0.5f
            );
            b.End();
            b.BeginDefault();
        }
        
        afterMenu:
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.6f);

        drawLeftMenu(b);
        drawRightMenu(b);
        drawCenterMenu(b);

        drawMouse(b);
    }

    private void drawCenterMenu(SpriteBatch b)
    {
        Game1.DrawBox(
            x: (int)_colourWheel.CenterPoint.X - width / 2 - borderWidth / 2,
            y: (int)_colourWheel.CenterPoint.Y - width / 2 - borderWidth / 2,
            width: width + borderWidth,
            height: height + borderWidth
        );

        _colourWheel.draw(b);
        _selectionCircle.draw(b);
        
        ToneSlider.draw(b, seeThrough: true);
        Rectangle toneTextBounds = GetColourTextBounds(0);
        
        float scaleMultiplier = (float)(1.0 + 0.05 * Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0));
        b.DrawString(
            spriteFont: Game1.smallFont,
            text: "Tone",
            position: new Vector2(toneTextBounds.X, toneTextBounds.Y),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: _activeColourIndex == 0 ? scaleMultiplier : 1f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        
        ShadingSlider.draw(b, seeThrough: true);
        Rectangle shadingTextBounds = GetColourTextBounds(1);
        b.DrawString(
            spriteFont: Game1.smallFont,
            text: "Shading",
            position: new Vector2(shadingTextBounds.X, shadingTextBounds.Y),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: _activeColourIndex == 1 ? scaleMultiplier : 1f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        if (_autoPalette)
        {
            b.Draw(
                texture: Game1.mouseCursors,
                position: new Vector2(ShadingSlider.Bar.Bounds.X, ShadingSlider.Bar.Bounds.Y),
                sourceRectangle: new Rectangle(269, 471, 14, 14),
                color: Color.White * 0.5f,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: new Vector2(ShadingSlider.Bar.Bounds.Width / 14f, ShadingSlider.Bar.Bounds.Height / 14f),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }
        
        OutlineSlider.draw(b, seeThrough: true);
        Rectangle outlineTextBounds = GetColourTextBounds(2);
        b.DrawString(
            spriteFont: Game1.smallFont,
            text: "Outline",
            position: new Vector2(outlineTextBounds.X, outlineTextBounds.Y),
            color: Game1.textColor,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: _activeColourIndex == 2 ? scaleMultiplier : 1f,
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
        if (_autoPalette)
        {
            b.Draw(
                texture: Game1.mouseCursors,
                position: new Vector2(OutlineSlider.Bar.Bounds.X, OutlineSlider.Bar.Bounds.Y),
                sourceRectangle: new Rectangle(269, 471, 14, 14),
                color: Color.White * 0.5f,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: new Vector2(OutlineSlider.Bar.Bounds.Width / 14f, OutlineSlider.Bar.Bounds.Height / 14f),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }

        _cancelButton.draw(b);
        _confirmButton.draw(b);
        
        // This is the "Settings" esque button in the top right that opens the advanced stuff (like the sliders and hex input).
        _toggleAdvancedControls.draw(b);
        // And this one is the one that toggles the preview of whatever it is we're colour picking for.
        _togglePreviewBase.draw(b);
        _togglePreviewIcon.draw(b);
    }

    private void drawLeftMenu(SpriteBatch b)
    {
        if (_drawPreview is null) return;
        
        Rectangle leftMenuBounds = GetLeftMenuBounds();
        Game1.DrawBox(
            x: leftMenuBounds.X,
            y: leftMenuBounds.Y,
            width: leftMenuBounds.Width,
            height: leftMenuBounds.Height
        );
        
        Rectangle safeLeftMenuBounds = GetSafeLeftMenuBounds();
        _drawPreview(b, safeLeftMenuBounds, GetPickedSkinTone(), _autoPalette);
    }

    private void drawRightMenu(SpriteBatch b)
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
        drawRightAutoPaletteToggle(b);
        drawRightDarkSkinToggle(b);
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

    private void drawRightAutoPaletteToggle(SpriteBatch b)
    {
        Rectangle autoPaletteHeaderBounds = GetAutoPaletteCheckboxBounds();
        _autoPaletteToggle.draw(b);
        Rectangle hexInputBounds = GetHexInputBounds();
        drawHeader(
            b: b,
            header: "Auto Palette",
            bounds: new Rectangle(
                x: hexInputBounds.X,
                y: autoPaletteHeaderBounds.Y + 2,
                width: autoPaletteHeaderBounds.Width - _autoPaletteToggle.bounds.Width - 8,
                height: autoPaletteHeaderBounds.Height
            ),
            includeLine: false
        );
    }
    
    private void drawRightDarkSkinToggle(SpriteBatch b)
    {
        Rectangle darkSkinHeaderBounds = GetDarkSkinCheckboxBounds();
        _darkSkinToggle.draw(b);
        Rectangle hexInputBounds = GetHexInputBounds();
        drawHeader(
            b: b,
            header: "Dark Skin",
            bounds: new Rectangle(
                x: hexInputBounds.X,
                y: darkSkinHeaderBounds.Y + 2,
                width: darkSkinHeaderBounds.Width - _darkSkinToggle.bounds.Width - 8,
                height: darkSkinHeaderBounds.Height
            ),
            includeLine: false
        );
    }

    private void drawRightHeaders(SpriteBatch b)
    {
        Rectangle rgbHeaderBounds = GetRgbHeaderBounds();
        drawHeader(b, "RGB", rgbHeaderBounds);
        
        Rectangle hsvHeaderBounds = GetHsvHeaderBounds();
        drawHeader(b, "HSV", hsvHeaderBounds);
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
}