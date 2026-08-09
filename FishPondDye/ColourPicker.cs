using System;
using FishPondDye.Components;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FishPondDye;

public class ColourPicker : IClickableMenu
{
    private FishPond? _pond = null;

    private readonly ColourWheel _colourWheel = new(
        new Vector2(
            x: Game1.uiViewport.Width / 2f,
            y: Game1.uiViewport.Height / 3f), 
        width: (int)(Game1.uiViewport.Height / 4f), 
        height: (int)(Game1.uiViewport.Height / 4f));
    
    private static Texture2D _selectionCircle = GenerateSelectionOutline();
    
    private float _value = 1f;
    
    private Color _previousColour = Color.White;
    private Color SelectedColour = Color.White;

    private bool _isSelecting = false;

    public ColourPicker(FishPond? pond)
    {
        _pond = pond;
    }
    
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        _isSelecting = false;
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);
        if (_isSelecting) SelectedColour = _colourWheel.GetColourAtPoint(new Vector2(x, y));
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        if (_colourWheel.Contains(new Vector2(x,y)))
        {
            _isSelecting = true;
            SelectedColour = _colourWheel.GetColourAtCursor();
            _previousColour = SelectedColour;
        }

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

    public static Texture2D GenerateSelectionOutline()
    {
        Texture2D outline = new Texture2D(Game1.graphics.GraphicsDevice, 8, 8, mipmap: false, format: SurfaceFormat.Color);
        byte[] shape =
        [
            0,0,1,1,1,1,0,0,
            0,1,0,0,0,0,1,0,
            1,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,1,
            1,0,0,0,0,0,0,1,
            0,1,0,0,0,0,1,0,
            0,0,1,1,1,1,0,0,
        ];
        Color[] data = new Color[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            data[i] = shape[i] == 1 ? Color.White : Color.Transparent;
        }
        outline.SetData(data);
        return outline;
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.4f);
        
        // draw a box in the center of the screen that is 1/3rd the screen width and half the screen height
        Game1.DrawBox(
            x: Game1.uiViewport.Width / 2 - Game1.uiViewport.Width / 6,
            y: Game1.uiViewport.Height / 2 - Game1.uiViewport.Width / 6,
            width: Game1.uiViewport.Width / 3,
            height: Game1.uiViewport.Width / 3
        );
        
        _colourWheel.draw(b);
        drawSelectionCircle(b);
        drawRgbBars(b);
        
        int rectWidth = 200;
        int rectHeight = 50;
        int rectX = Game1.uiViewport.Width / 2 - rectWidth / 2;
        int rectY = (int)_colourWheel.CenterPoint.Y + (int)_colourWheel.Height / 2 + 20;

        // b.Draw(Game1.staminaRect, new Rectangle(rectX, rectY, rectWidth, rectHeight), color: SelectedColour);
        drawMouse(b);
    }

    public void drawRgbBars(SpriteBatch b)
    {
        Vector2 rgbStrings = Game1.smallFont.MeasureString("R:");
        int barWidth = Game1.uiViewport.Width / 7 - (int)rgbStrings.X;
        int barHeight = (int)(_colourWheel.Height / 6 / 1.5f);
        int barSpacing = 8;
        float leftEdge = _colourWheel.CenterPoint.X + barWidth / 2f + rgbStrings.X * 2.25f;
        float topEdge = _colourWheel.CenterPoint.Y - _colourWheel.Height / 2f;
        float bottomEdge = _colourWheel.CenterPoint.Y + _colourWheel.Height / 2f;

        Vector2 rBar = new Vector2(leftEdge, topEdge);
        Vector2 gBar = new Vector2(leftEdge, topEdge + barHeight + barSpacing);
        Vector2 bBar = new Vector2(leftEdge, topEdge + (barHeight + barSpacing) * 2);

        b.Draw(Game1.staminaRect, new Rectangle((int)rBar.X, (int)rBar.Y, width: barWidth, height: barHeight), color: Color.Red);
        b.DrawString(spriteFont: Game1.smallFont, text: "R:", position: new Vector2(rBar.X - rgbStrings.X * 1.25f, rBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        b.Draw(Game1.staminaRect, new Rectangle((int)gBar.X, (int)gBar.Y, width: barWidth, height: barHeight), color: Color.Green);
        b.DrawString(spriteFont: Game1.smallFont, text: "G:", position: new Vector2(gBar.X - rgbStrings.X * 1.325f, gBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        b.Draw(Game1.staminaRect, new Rectangle((int)bBar.X, (int)bBar.Y, width: barWidth, height: barHeight), color: Color.Blue);
        b.DrawString(spriteFont: Game1.smallFont, text: "B:", position: new Vector2(bBar.X - rgbStrings.X * 1.25f, bBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
        Vector2 hBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing) * 2);
        Vector2 sBar = new Vector2(leftEdge, bottomEdge - barHeight - (barHeight + barSpacing));
        Vector2 vBar = new Vector2(leftEdge, bottomEdge - barHeight);
        
        b.Draw(Game1.staminaRect, new Rectangle((int)hBar.X, (int)hBar.Y, width: barWidth, height: barHeight), color: Color.Red);
        b.DrawString(spriteFont: Game1.smallFont, text: "H:", position: new Vector2(hBar.X - rgbStrings.X * 1.25f, hBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        b.Draw(Game1.staminaRect, new Rectangle((int)sBar.X, (int)sBar.Y, width: barWidth, height: barHeight), color: Color.Green);
        b.DrawString(spriteFont: Game1.smallFont, text: "S:", position: new Vector2(sBar.X - rgbStrings.X * 1.25f, sBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        b.Draw(Game1.staminaRect, new Rectangle((int)vBar.X, (int)vBar.Y, width: barWidth, height: barHeight), color: Color.Blue);
        b.DrawString(spriteFont: Game1.smallFont, text: "V:", position: new Vector2(vBar.X - rgbStrings.X * 1.25f, vBar.Y + barHeight / 2f - rgbStrings.Y / 2f), color: Game1.textColor);
        
    }

    public void drawSelectionCircle(SpriteBatch b)
    {
        Vector2 selectionPos = ColourWheel.RGBToPoint(SelectedColour);
        selectionPos = new Vector2(
            x: _colourWheel.CenterPoint.X + selectionPos.X * (_colourWheel.Width / 2f),
            y: _colourWheel.CenterPoint.Y + selectionPos.Y * (_colourWheel.Height / 2f)
        );
        b.Draw(
            texture: _selectionCircle,
            position: selectionPos,
            sourceRectangle: null,
            color: Color.Black * 0.8f,
            rotation: 0f,
            scale: 2.5f,
            origin: new Vector2(_selectionCircle.Width / 2f, _selectionCircle.Height / 2f),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
            
        b.Draw(
            texture: _selectionCircle,
            position: selectionPos,
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            scale: 2f,
            origin: new Vector2(_selectionCircle.Width / 2f, _selectionCircle.Height / 2f),
            effects: SpriteEffects.None,
            layerDepth: 1f
        );
    }

    public override void update(GameTime time)
    {
        base.update(time);
        _value = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000f) * 0.5f + 0.5f;
        var smoothing = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 10f) * 0.5f + 0.5f;
        _colourWheel.Smoothing = smoothing;
        _colourWheel.Value = 1.0f;
        UpdateWheelCenter();
    }

    public void UpdateWheelCenter()
    {
        _colourWheel.CenterPoint = new Vector2(Game1.uiViewport.Width / 2f - Game1.uiViewport.Width / 13f, Game1.uiViewport.Height / 2f - Game1.uiViewport.Width / 13f);
        _colourWheel.Width = Game1.uiViewport.Width / 7f;
        _colourWheel.Height = Game1.uiViewport.Width / 7f;
    }
}