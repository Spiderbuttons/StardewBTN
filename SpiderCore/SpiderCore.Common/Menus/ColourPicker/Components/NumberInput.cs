using System;
using System.Globalization;
using SpiderCore.Common.Colour;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SpiderCore.Common.Menus.ColourPicker.Components
{
    public class NumberInput : TextBox
    {
        Texture2D inputTexture => Game1.menuTexture;
        Rectangle sourceRect = new Rectangle(0, 320, 60, 60);
        private Rectangle leftEdgeRect = new(0, 320, 9, 60);
        private Rectangle centerRect = new(9, 320, 42, 60);
        private Rectangle rightEdgeRect = new(51, 320, 9, 60);

        private Rectangle minusSourceRect = new(177, 345, 7, 8);
        private Rectangle plusSourceRect = new(184, 345, 7, 8);
    
        private readonly Func<decimal>? _getBackingValue;
        private readonly Action<decimal>? _setBackingValue;

        private int Min, Max;
    
        private float timeHeldDown = 0f;
        private float timeBeforeFastInput = 500f;
        private float timeBetweenInputTicks = 25f;
    
        public ClickableTextureComponent upButton = new(
            name: "upButton",
            bounds: new Rectangle(0, 0, 7, 8),
            label: null,
            hoverText: null,
            texture: Game1.mouseCursors,
            sourceRect: new Rectangle(184, 345, 7, 8),
            scale: 1f
        )
        {
            myID = 90,
            region = 1
        };
    
        public ClickableTextureComponent downButton = new(
            name: "downButton",
            bounds: new Rectangle(0, 0, 7, 8),
            label: null,
            hoverText: null,
            texture: Game1.mouseCursors,
            sourceRect: new Rectangle(177, 345, 7, 8),
            scale: 1f
        )
        {
            myID = 91,
            region = 1
        };
    
        public NumberInput(SpriteFont font, Color textColor, Func<decimal>? getBackingValue = null, Action<decimal>? setBackingValue = null, int min = 0, int max = int.MaxValue) : base(null, null,
            font,
            textColor)
        {
            numbersOnly = true;
            _getBackingValue = getBackingValue;
            _setBackingValue = setBackingValue;
            Min = min;
            Max = max;
        }
    
        public void UpdateButtonPositions()
        {
            upButton.scale = upButton.baseScale = GetPlusMinusScale();
            downButton.scale = downButton.baseScale = GetPlusMinusScale();
        
            upButton.bounds = GetPlusMinusBounds();
            downButton.bounds = GetPlusMinusBounds(isPlus: false);
        }

        public float GetPlusMinusScale()
        {
            float buttonHeight = Height / 2f;
            return buttonHeight / upButton.sourceRect.Height;
        }

        public Rectangle GetPlusMinusBounds(bool isPlus = true)
        {
            float buttonHeight = Height / 2f;
            int yCoord = isPlus ? Y : Y + (int)buttonHeight;
            return new Rectangle(
                x: (int)(X + Width - upButton.sourceRect.Width * GetPlusMinusScale()),
                y: yCoord, 
                width: (int)(upButton.sourceRect.Width * GetPlusMinusScale()),
                height: (int)buttonHeight);
        }
    
        public void receiveLeftClick(int x, int y)
        {
            timeHeldDown = 0;
        
            int amountToChange = GetPlusMinusBounds().Contains(x, y) ? 1 : GetPlusMinusBounds(isPlus: false).Contains(x, y) ? -1 : 0;
            if (isHoldingModifierKey()) amountToChange *= 10;
        
            if (amountToChange != 0)
            {
                decimal previousValue = _getBackingValue?.Invoke() ?? 0;
                _setBackingValue?.Invoke(Math.Clamp((_getBackingValue?.Invoke() ?? 0) + amountToChange, Min, Max));
                if (previousValue != _getBackingValue?.Invoke()) Game1.playSound("dialogueCharacter");
            }
        }

        public void leftClickHeld(int x, int y)
        {
            int amountToChange = GetPlusMinusBounds().Contains(x, y) ? 1 : GetPlusMinusBounds(isPlus: false).Contains(x, y) ? -1 : 0;
            if (isHoldingModifierKey()) amountToChange *= 10;
        
            if (amountToChange != 0)
            {
                timeHeldDown += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                if (timeHeldDown >= timeBeforeFastInput)
                {
                    if (timeHeldDown >= timeBeforeFastInput + timeBetweenInputTicks)
                    {
                        decimal previousValue = _getBackingValue?.Invoke() ?? 0;
                        _setBackingValue?.Invoke(Math.Clamp((_getBackingValue?.Invoke() ?? 0) + amountToChange, Min, Max));
                        if (previousValue != _getBackingValue?.Invoke()) Game1.playSound("dialogueCharacter");
                        timeHeldDown = timeBeforeFastInput;
                    }
                }
            }
            else timeHeldDown = 0f;
        }

        private static bool isHoldingModifierKey()
        {
            KeyboardState kb = Game1.input.GetKeyboardState();
            if (kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift))
                return true;

            GamePadState gp = Game1.input.GetGamePadState();
            return gp.IsButtonDown(Buttons.LeftShoulder) || gp.IsButtonDown(Buttons.RightShoulder);
        }

        public bool containsPoint(int x, int y)
        {
            return new Rectangle(X, Y, Width, Height).Contains(x, y) || GetPlusMinusBounds().Contains(x, y) || GetPlusMinusBounds(isPlus: false).Contains(x, y);
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
        {
            bool caretVisible = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
            string toDraw = Math.Round(_getBackingValue?.Invoke() ?? -1).ToString(CultureInfo.InvariantCulture);
        
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

            upButton?.draw(spriteBatch);
            downButton?.draw(spriteBatch);
        
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
        
            float scale = Math.Min(Height / size.Y, Width * 0.75f / size.X);
            Utility.drawTextWithShadow(
                b: spriteBatch,
                text: toDraw,
                font: _font,
                position: new Vector2(
                    x: X + (Width / 14f),
                    y: Y + (Height / 1.75f) - (size.Y * scale / 2f)
                ),
                color: _textColor,
                scale: scale,
                layerDepth: 1f
            );
        }
    }
}