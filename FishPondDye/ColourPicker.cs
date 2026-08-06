using System;
using System.Collections.Generic;
using System.Diagnostics;
using FishPondDye.Components;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace FishPondDye;

public class ColourPicker : IClickableMenu
{
    private FishPond? _pond = null;

    private readonly ColourWheel _colourWheel = new(2048, 2048);
    private float _value = 1f;

    public ColourPicker(FishPond? pond)
    {
        _pond = pond;
    }
    
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key)
    {
        base.receiveKeyPress(key);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.4f);
        
        float x = Game1.uiViewport.Width / 2f;
        float y = Game1.uiViewport.Height / 2.5f;
        
        // Idk 40% of the viewport height seems fine.
        float scale = Math.Min(Game1.uiViewport.Width / (float)_colourWheel.Width, Game1.uiViewport.Height / (float)_colourWheel.Height) * 0.4f;
        
        _colourWheel.draw(b, new Vector2(x, y), scale);
        
        drawMouse(b);
    }

    public override void update(GameTime time)
    {
        base.update(time);
        // _value = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000f) * 0.5f + 0.5f;
        // var smoothing = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000f) * 0.5f + 0.5f;
        _colourWheel.Smoothing = 1.0f;
    }
}