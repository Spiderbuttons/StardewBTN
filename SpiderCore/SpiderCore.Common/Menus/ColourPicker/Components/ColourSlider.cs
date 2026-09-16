using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpiderCore.Common.Menus.ColourPicker.Components
{
    public class ColourSlider : ClickableComponent
    {
        private static readonly Rectangle CapSourceRect = new(435, 463, 6, 1);
        private static readonly Rectangle MiddleSourceRect = new(435, 464, 6, 8);

        private readonly decimal _min;
        private readonly decimal _max;

        private float timeUntilNextSound = 50f;
    
        private readonly Func<decimal>? _getBackingValue;
        private readonly Action<decimal>? _setBackingValue;
        public float Progress
        {
            get
            {
                if (_getBackingValue is null) return 0f;
            
                float progress = (float)((_getBackingValue() - _min) / (_max - _min));
                return Math.Clamp(progress, -1f, 1f);
            }
            set
            {
                if (_setBackingValue is null) return;
            
                if (!IsHorizontal) value = 1f - value;
                _setBackingValue(Math.Clamp(_min + (decimal)value * (_max - _min), _min, _max));
            }
        }

        private float GrabberScale => IsHorizontal ? (float)Bar.Bounds.Height / MiddleSourceRect.Height : (float)Bar.Bounds.Width / MiddleSourceRect.Height;

        public bool Selected = false;

        public bool IsHorizontal
        {
            get => Bar.IsHorizontal;
            set => Bar.IsHorizontal = value;
        }
    
        public bool IsHueBar 
        {
            get => Bar.IsHueBar;
            set => Bar.IsHueBar = value;
        }
    
        public bool IsAlphaBar 
        {
            get => Bar.IsAlphaBar;
            set => Bar.IsAlphaBar = value;
        }
    
        public GradientBar Bar;
        public NumberInput? Input;
    
        public ColourSlider(string name, Func<decimal>? getBackingValue, Action<decimal>? setBackingValue, decimal min = 0, decimal max = 100, Rectangle? bounds = null, Color? colourOne = null, Color? colourTwo = null) : base(bounds ?? Rectangle.Empty, name)
        {
            _getBackingValue = getBackingValue;
            _setBackingValue = setBackingValue;
            _min = min;
            _max = max;
            Bar = new GradientBar(bounds, colourOne, colourTwo);
        
            if (getBackingValue is null || setBackingValue is null) return;
        
            Input = new NumberInput(Game1.smallFont, Color.Black, getBackingValue, setBackingValue, (int)min, (int)max)
            {
                X = bounds?.Right ?? 0,
                Y = bounds?.Y ?? 0,
                Width = 60,
                Height = bounds?.Height ?? 0,
                Text = _getBackingValue?.Invoke().ToString(CultureInfo.InvariantCulture) ?? string.Empty
            };
        }

        public void receiveLeftClick(int x, int y)
        {
            if (Bar.containsPoint(x, y) || GetGrabberBounds().Contains(x, y))
            {
                Selected = true;
                Game1.playSound("button_tap");
                timeUntilNextSound = 50f;
            }
            if (Input?.containsPoint(x, y) == true) Input.receiveLeftClick(x, y);
        }

        public void leftClickHeld(int x, int y)
        {
            if (!Selected)
            {
                if (Input?.containsPoint(x, y) == true) Input.leftClickHeld(x, y);
                return;
            }

            float progressLastTick = Progress;
            float progress;
            if (IsHorizontal)
            {
                progress = (float)(x - Bar.Bounds.X) / Bar.Bounds.Width;
            }
            else
            {
                progress = (float)(y - Bar.Bounds.Y) / Bar.Bounds.Height;
            }
            float clamped = Math.Clamp(progress, 0f, 1f);
            Progress = clamped;
            if (timeUntilNextSound <= 0f && progressLastTick != Progress)
            {
                Game1.playSound("button_tap");
                timeUntilNextSound = 50f;
            } else if (timeUntilNextSound > 0f)
            {
                timeUntilNextSound -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            }
        }

        public void UpdateColours(Color one, Color two)
        {
            Bar.ColourOne = one;
            Bar.ColourTwo = two;
        }

        public void UpdateBarBounds(Rectangle newBounds)
        {
            Bar.Bounds = newBounds;
            bounds = newBounds;
        }

        public void UpdateInputBounds(Rectangle newBounds)
        {
            if (Input is null) return;
        
            Input.X = newBounds.Left;
            Input.Y = newBounds.Y;
            Input.Width = newBounds.Width;
            Input.Height = newBounds.Height;
        }

        public Vector2 GetGrabberCenter()
        {
            if (Bar.Bounds.IsEmpty) return Vector2.Zero;

            if (IsHorizontal)
            {
                float x = Bar.Bounds.Left + Progress * Bar.Bounds.Width;
                float y = Bar.Bounds.Center.Y;
                if (Bar.Bounds.Height % 2 != 0) y += 0.5f;
                return new Vector2(x, y);
            }
            else
            {
                float x = Bar.Bounds.Center.X;
                float y = Bar.Bounds.Bottom - Progress * Bar.Bounds.Height - (MiddleSourceRect.Width - CapSourceRect.Height) * GrabberScale / 2f;
                return new Vector2(x, y);
            }
        }
    
        public Rectangle GetGrabberBounds() 
        {
            Vector2 grabberCenter = GetGrabberCenter();
            if (IsHorizontal)
            {
                return new Rectangle(
                    (int)(grabberCenter.X - MiddleSourceRect.Width * GrabberScale / 2f),
                    (int)(grabberCenter.Y - MiddleSourceRect.Height * GrabberScale / 2f - CapSourceRect.Height * 2f),
                    (int)(MiddleSourceRect.Width * GrabberScale),
                    (int)(MiddleSourceRect.Height * GrabberScale + CapSourceRect.Height * 4f)
                );
            }

            return new Rectangle(
                (int)(grabberCenter.X - MiddleSourceRect.Height * GrabberScale / 2f - CapSourceRect.Height),
                (int)(grabberCenter.Y - MiddleSourceRect.Width * GrabberScale / 2f),
                (int)(MiddleSourceRect.Height * GrabberScale + CapSourceRect.Height * 2f),
                (int)(MiddleSourceRect.Width * GrabberScale)
            );
        }

        public override bool containsPoint(int x, int y)
        {
            return barContainsPoint(x, y) || inputContainsPoint(x, y);
        }

        public bool barContainsPoint(int x, int y)
        {
            return Bar.Bounds.Contains(x, y) || GetGrabberBounds().Contains(x, y);
        }
    
        public bool inputContainsPoint(int x, int y)
        {
            return Input?.containsPoint(x, y) == true;
        }

        public void draw(SpriteBatch b, bool seeThrough = false)
        {
            if (Bar.Bounds.IsEmpty) return;
        
            Bar.draw(b, seeThrough);
            if (_setBackingValue is not null) drawSliderGrabber(b);

            Input?.Draw(b);
        }
    
        public void drawSliderGrabber(SpriteBatch b)
        {
            Color grabberColour = Color.White;
            Vector2 center = GetGrabberCenter();

            if (IsHorizontal)
            {
                // Top Piece
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: center - new Vector2(0, MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height * 2f),
                    sourceRectangle: CapSourceRect,
                    color: grabberColour,
                    rotation: 0f,
                    origin: new Vector2(CapSourceRect.Width / 2f, 0),
                    scale: 2f,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            
                // Middle Piece
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: center,
                    sourceRectangle: MiddleSourceRect,
                    color: grabberColour,
                    rotation: 0f,
                    origin: new Vector2(MiddleSourceRect.Width / 2f, MiddleSourceRect.Height / 2f),
                    scale: new Vector2(2f, GrabberScale),
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            
                // Bottom Piece
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: center + new Vector2(0, MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height / 2f),
                    sourceRectangle: CapSourceRect,
                    color: grabberColour,
                    rotation: 0f,
                    origin: new Vector2(CapSourceRect.Width / 2f, 0),
                    scale: 2f,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            }
            else
            {
                // Left Piece
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: center - new Vector2(MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height, 0),
                    sourceRectangle: CapSourceRect,
                    color: grabberColour,
                    rotation: MathHelper.ToRadians(90f),
                    origin: new Vector2(0, CapSourceRect.Height / 2f),
                    scale: 2f,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            
                // Middle Piece
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: center,
                    sourceRectangle: MiddleSourceRect,
                    color: grabberColour,
                    rotation: MathHelper.ToRadians(90f),
                    origin: new Vector2(0, MiddleSourceRect.Height / 2f),
                    scale: new Vector2(2f, GrabberScale),
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            
                // Right Piece
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: center + new Vector2(MiddleSourceRect.Height * GrabberScale / 2f + CapSourceRect.Height, 0),
                    sourceRectangle: CapSourceRect,
                    color: grabberColour,
                    rotation: MathHelper.ToRadians(90f),
                    origin: new Vector2(0, CapSourceRect.Height / 2f),
                    scale: 2f,
                    effects: SpriteEffects.None,
                    layerDepth: 1f
                );
            }
        }
    }
}