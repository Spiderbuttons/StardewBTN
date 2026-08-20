using FishPondDye.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
    public override void receiveKeyPress(Keys key)
    {
        if (key != Keys.None && Game1.options.doesInputListContain(Game1.options.menuButton, key))
        {
            _hexInput.Selected = false;
        }
        base.receiveKeyPress(key);
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        _colourWheel.Selected = false;
        foreach (var (_, slider) in _sliders)
        {
            slider.Selected = false;
        }
        _selectedSlider = null;
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);

        if (_colourWheel.Selected)
        {
            HsvColour colourBeforeChange = PickedColourHsv;
            HsvColour hsv = _colourWheel.GetColourAtScreenPoint(new Vector2(x, y)).ToHsv();
            _hue = hsv.Hue;
            _saturation = hsv.Saturation;
            if (timeUntilNextSound <= 0 && colourBeforeChange != PickedColourHsv)
            {
                Game1.playSound("button_tap");
                timeUntilNextSound = 50f;
            } else timeUntilNextSound -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;

            return;
        }

        foreach (var (_, slider) in _sliders)
        {
            slider.leftClickHeld(x, y);
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        _colourWheel.Selected = _colourWheel.containsPoint(x, y);
        if (_colourWheel.Selected)
        {
            if (_value is 0) _value = 100M;
            Game1.playSound("button_tap");
            timeUntilNextSound = 50f;
        }
        
        for (var i = 0; i < _palette.Count; i++)
        {
            var square = _palette[i];
            if (!square.containsPoint(x, y)) continue;
            
            if (square.IsAddSquare)
            {
                AddColourToPalette(PickedColourHsv);
                Game1.playSound("shwip");
                break;
            }

            if (square is { StoredColour: not null })
            {
                SetColour(HsvColour.FromXnaColor(square.StoredColour.Value));
            } else SetColour(HsvColour.FromXnaColor(Color.White));
            

            Game1.playSound("smallSelect");
        }
        
        if (_toggleAdvancedControls.containsPoint(x, y))
        {
            _toggleAdvancedControls.scale = _toggleAdvancedControls.baseScale * 0.975f;
            _showingAdvancedControls = !_showingAdvancedControls;
            Game1.playSound("drumkit6");
        }
        
        if (_togglePreviewBase.containsPoint(x, y))
        {
            _togglePreviewBase.scale = _togglePreviewBase.baseScale * 0.975f;
            _togglePreviewIcon.scale = _togglePreviewIcon.baseScale * 0.975f;
            Game1.playSound("drumkit6");
        }

        if (!_showingAdvancedControls || _rightSectionOffset.X < width * 0.9f) return;
        
        foreach (var (_, slider) in _sliders)
        {
            slider.receiveLeftClick(x, y);
            if (slider.Selected)
            {
                _selectedSlider = slider;
                break;
            }
        }

        if (_hexInput.containsPoint(x, y))
        {
            _hexInput.Text = PickedColourRgb.ToHexString()[..6];
            _hexInput.SelectMe();
            _hexInput.caretTimer = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            Game1.playSound("dialogueCharacter");
        }
        else _hexInput.Selected = false;
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
        for (var i = 0; i < _palette.Count; i++)
        {
            var square = _palette[i];
            if (!square.containsPoint(x, y)) continue;
            
            if (square.IsAddSquare || square.Locked || !square.StoredColour.HasValue) return;
            
            RemoveColourFromPalette(i);
            Game1.playSound("dwoop");
        }
        
        Log.Info(PickedColourHsv.ToXnaColor());
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);
        _toggleAdvancedControls.tryHover(x, y);
        _togglePreviewBase.tryHover(x, y);
    }
}