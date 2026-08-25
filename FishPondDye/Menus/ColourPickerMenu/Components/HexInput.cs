using System;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu.Components;

public class HexInput : TextBox
{
    Texture2D inputTexture => Game1.menuTexture;
    Rectangle sourceRect = new Rectangle(0, 320, 60, 60);
    private Rectangle leftEdgeRect = new(0, 320, 9, 60);
    private Rectangle centerRect = new(9, 320, 42, 60);
    private Rectangle rightEdgeRect = new(51, 320, 9, 60);
    
    Func<RgbColour> _getColour;
    Action<RgbColour> _setColour;

    public double caretTimer = 0f;
    
    public HexInput(SpriteFont font, Color textColor, Func<RgbColour> getColour, Action<RgbColour> setColour) : base(null,
        null,
        font,
        textColor)
    {
        _getColour = getColour;
        _setColour = setColour;
        textLimit = 6;
        limitWidth = false;
        Text = getColour().ToHexString();
    }

    public bool containsPoint(int x, int y)
    {
        return GetBounds().Contains(new Point(x, y));
    }
    
    public Rectangle GetBounds()
    {
        return new Rectangle(X, Y, Width, Height);
    }

    public override void RecieveTextInput(char inputChar)
    {
        if (!Uri.IsHexDigit(inputChar)) return;
        inputChar = char.ToUpper(inputChar);
        if (Text.Length < textLimit)
        {
            Text += inputChar;
            Game1.playSound("dialogueCharacter");
            SetColourFromHex(Text);
        }
    }
    
    public override void RecieveCommandInput(char command)
    {
        if (!Selected || command is not '\b' and not '\r')
        {
            base.RecieveCommandInput(command);
            return;
        }

        switch (command)
        {
            case '\b':
                OnBackspace();
                break;
            case '\r':
                Selected = false;
                if (!Game1.options.SnappyMenus) Game1.playSound("drumkit6");
                break;
        }
    }

    public override void RecieveTextInput(string text)
    {
        base.RecieveTextInput(text);
    }

    public void SetColourFromHex(string hex)
    {
        if (hex.Length > 6) hex = hex[..6];
        Text = hex;
        while (hex.Length < 6) hex += "0";
        RgbColour newColour = RgbColour.FromHexString(hex);
        RgbColour currentColour = _getColour();
        _setColour(new RgbColour(newColour.R, newColour.G, newColour.B, currentColour.A));
    }

    public void OnBackspace()
    {
        if (Text.Length > 0)
        {
            Text = Text[..^1];
            Game1.playSound("tinyWhip");
            SetColourFromHex(Text);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
    {
        bool caretVisible = (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - caretTimer) % 1000.0 < 500.0;
        RgbColour rgb = _getColour();
        string toDraw = Selected ? Text : rgb.ToHexString()[..6];
        
        spriteBatch.Draw(
            texture: inputTexture,
            destinationRectangle: new Rectangle(X, Y, leftEdgeRect.Width / 2, Height),
            sourceRectangle: leftEdgeRect,
            color: Color.White
        );
        
        spriteBatch.Draw(
            texture: inputTexture,
            destinationRectangle: new Rectangle(X + leftEdgeRect.Width / 2, Y, Width - leftEdgeRect.Width / 2 - rightEdgeRect.Width / 2, Height),
            sourceRectangle: centerRect,
            color: Color.White
        );
        
        spriteBatch.Draw(
            texture: inputTexture,
            destinationRectangle: new Rectangle(X + Width - rightEdgeRect.Width / 2, Y, rightEdgeRect.Width / 2, Height),
            sourceRectangle: rightEdgeRect,
            color: Color.White
        );
        
        Vector2 size = _font.MeasureString(text: toDraw);
        while (size.X > Width)
        {
            toDraw = toDraw[1..];
            size = _font.MeasureString(text: toDraw);
        }
        
        Vector2 baseSize = _font.MeasureString("BBBBBB");
        Vector2 hashTagSize = _font.MeasureString("#");
        float scale = Math.Min(Height / baseSize.Y, Width * 0.75f / baseSize.X);
        
        Utility.drawTextWithShadow(
            b: spriteBatch,
            text: "#",
            font: _font,
            position: new Vector2(
                x: X + (hashTagSize.X) / 3f,
                y: Y + hashTagSize.Y * scale / 10f
            ),
            color: _textColor,
            scale: scale,
            layerDepth: 1f
        );
        
        Utility.drawTextWithShadow(
            b: spriteBatch,
            text: toDraw,
            font: _font,
            position: new Vector2(
                x: X + (baseSize.X / 6) / 2f + hashTagSize.X * scale,
                y: Y + size.Y * scale / 10f
            ),
            color: _textColor,
            scale: scale,
            layerDepth: 1f
        );
        
        if (caretVisible && Selected)
        {
            spriteBatch.Draw(
                texture: Game1.staminaRect,
                destinationRectangle: new Rectangle(
                    x: X + (int)(((size.X + hashTagSize.X) * scale) + (hashTagSize.X / 2.25f)),
                    y: Y + (int)(size.Y * scale * 0.15f),
                    width: 4,
                    height: (int)(size.Y * scale * 0.725f)
                ),
                color: _textColor
            );
        }
    }
}