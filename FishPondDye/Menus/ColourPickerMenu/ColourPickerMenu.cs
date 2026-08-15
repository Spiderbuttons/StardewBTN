using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu : IClickableMenu
{
    private static Texture2D _selectionCircle
    {
        get
        {
            if (field is not null) return field;
            
            field = new Texture2D(Game1.graphics.GraphicsDevice, 9, 9);
            byte[] shape =
            [
                0,0,1,1,1,1,1,0,0,
                0,1,3,3,3,3,3,1,0,
                1,3,0,0,0,0,0,3,1,
                1,4,0,0,0,0,0,4,1,
                2,4,0,0,0,0,0,4,2,
                2,4,0,0,0,0,0,4,2,
                2,5,0,0,0,0,0,5,2,
                0,2,6,5,5,5,6,2,0,
                0,0,2,2,2,2,2,0,0
            ];
            Color[] data = new Color[shape.Length];
            for (int i = 0; i < shape.Length; i++)
            {
                data[i] = shape[i] switch 
                {
                    1 => new Color(88, 29, 43),
                    2 => new Color(29, 17, 9),
                    3 => new Color(251, 249, 0) * 0.8f,
                    4 => new Color(242, 188, 82) * 0.8f,
                    5 => new Color(220, 123, 5) * 0.8f,
                    6 => new Color(177, 78, 5) * 0.8f,
                    _ => Color.Transparent
                };
            }
            field.SetData(data);
            return field;
        }
    }
    private Vector2 _screenCenter => new(Game1.uiViewport.Width / 2f, Game1.uiViewport.Height / 2f);

    private ColourWheel _colourWheel;

    private decimal _hue;
    private decimal _saturation;
    private decimal _value;
    private decimal _alpha;
    
    public HsvColour PickedColourHsv => new(_hue, _saturation, _value, _alpha);
    public RgbColour PickedColourRgb => PickedColourHsv.ToRgb();

    private FishPond? _pond = null;

    public ColourPickerMenu()
    {
        width = Game1.uiViewport.Width / 4;
        height = Game1.uiViewport.Height / 3;
        _colourWheel = new ColourWheel(_screenCenter, width, height);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        base.gameWindowSizeChanged(oldBounds, newBounds);
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

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.4f);
        
        Game1.DrawBox(
            x: (int)_screenCenter.X - width / 2,
            y: (int)_screenCenter.Y - height / 2,
            width: width,
            height: height,
            color: new Color(84, 35, 15) * 0.4f
        );

        _colourWheel.draw(b);

        drawMouse(b);
    }

    public override void update(GameTime time)
    {
        base.update(time);
    }
}