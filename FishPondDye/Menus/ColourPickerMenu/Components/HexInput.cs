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
    
    public HexInput(SpriteFont font, Color textColor) : base(null,
        null,
        font,
        textColor)
    {
    }

    public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
    {
        bool caretVisible = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
        string toDraw = Text.ToUpperInvariant();
        
        if (toDraw[index: 0] != '#') toDraw = "#" + toDraw;
        
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
        if (caretVisible && Selected)
        {
            spriteBatch.Draw(
                texture: Game1.staminaRect,
                destinationRectangle: new Rectangle(
                    x: X + 16 + (int)size.X + 2,
                    y: Y + 8,
                    width: 4,
                    height: 16
                ),
                color: _textColor
            );
        }
        
        if (drawShadow)
        {
            Utility.drawTextWithShadow(
                b: spriteBatch,
                text: toDraw,
                font: _font,
                position: new Vector2(
                    x: X + 16,
                    y: Y + (_textBoxTexture != null ? 12 : 8)
                ),
                color: _textColor,
                scale: 0.5f
            );
        }
        else {
            spriteBatch.DrawString(
                spriteFont: _font,
                text: toDraw,
                position: new Vector2(
                    x: X + 16,
                    y: Y + (_textBoxTexture != null ? 12 : 8)
                ),
                color: _textColor,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: 0.99f
            );
        }
    }
}