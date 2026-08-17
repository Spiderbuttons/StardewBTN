using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu.Components;

public class PaletteSquare : ClickableComponent
{
    public Color? StoredColour { get; set; }
    public bool Locked { get; set; }
    public bool IsAddSquare = false;
    
    public PaletteSquare(string name, Rectangle? bounds = null, Color? storedColour = null, bool locked = false) : base(bounds ?? Rectangle.Empty, name)
    {
        StoredColour = storedColour;
        Locked = locked;
    }
    
    public void draw(SpriteBatch b)
    {
        if (StoredColour.HasValue)
        {
            b.Draw(
                texture: Game1.staminaRect,
                position: new Vector2(bounds.X, bounds.Y),
                sourceRectangle: null,
                color: StoredColour.Value,
                rotation: 0f,
                scale: (int)scale,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }
        else
        {
            b.Draw(
                texture: Game1.staminaRect,
                position: new Vector2(bounds.X, bounds.Y),
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                scale: (int)scale,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }

        Color borderColour = new Color(104, 57, 36);
        int borderWidth = 2;
        
        // Left Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(bounds.Left, bounds.Y, borderWidth, bounds.Height),
            color: borderColour * 0.7f
        );
        // Right Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(bounds.Right - borderWidth, bounds.Y, borderWidth, bounds.Height),
            color: borderColour * 0.9f
        );
        // Top Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(bounds.X + borderWidth, bounds.Y, bounds.Width - borderWidth * 2, borderWidth),
            color: borderColour * 0.7f
        );
        // Bottom Border
        b.Draw(
            texture: Game1.staminaRect,
            destinationRectangle: new Rectangle(bounds.X + borderWidth, bounds.Bottom - borderWidth, bounds.Width - borderWidth * 2, borderWidth),
            color: borderColour * 0.9f
        );
        
        if (IsAddSquare)
        {
            b.Draw(
                texture: Game1.mouseCursors,
                position: new Vector2(bounds.Center.X, bounds.Center.Y),
                sourceRectangle: new Rectangle(
                    x: 0,
                    y: 428,
                    width: 10,
                    height: 10),
                color: Color.White * 0.3f,
                rotation: 0f,
                origin: new Vector2(5f, 5f),
                scale: scale / 10f / 2f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
        }
    }
}