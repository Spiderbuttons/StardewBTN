using System;
using System.Linq;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace FishPondDye.Menus.ColourPickerMenu;

public partial class ColourPickerMenu
{
    public override void applyMovementKey(int direction)
    {
        base.applyMovementKey(direction);
    }
    
    public override void snapCursorToCurrentSnappedComponent()
    {
        if (getCurrentlySnappedComponent() == _colourWheel)
        {
            Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
            Point position = new Point(
                x: (int)Math.Round(_colourWheel.CenterPoint.X + point.X * _colourWheel.Width / 2f),
                y: (int)Math.Round(_colourWheel.CenterPoint.Y + point.Y * _colourWheel.Height / 2f)
            );
            
            Game1.setMousePosition(position, true);
            return;
        }
        
        base.snapCursorToCurrentSnappedComponent();
    }

    public override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
    {
        base.noSnappedComponentFound(direction, oldRegion, oldID);
    }

    public override void customSnapBehavior(int direction, int oldRegion, int oldID)
    {
        base.customSnapBehavior(direction, oldRegion, oldID);
    }

    public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
    {
        base.automaticSnapBehavior(direction, oldRegion, oldID);
    }

    public override bool areGamePadControlsImplemented()
    {
        return true;
        return base.areGamePadControlsImplemented();
    }

    public override void receiveGamePadButton(Buttons button)
    {
        Point mousePosition = Game1.getMousePosition();
        if (_colourWheel.containsPoint(mousePosition.X, mousePosition.Y))
        {
            if (button.ToSButton().IsActionButton())
            {
                _colourWheel.Selected = true;
                Game1.playSound("button_tap");
            }
        }
    }

    public override void snapToDefaultClickableComponent()
    {
        currentlySnappedComponent = getComponentWithID(0);
        snapCursorToCurrentSnappedComponent();
    }

    public override void gamePadButtonHeld(Buttons b)
    {
        base.gamePadButtonHeld(b);
        if (!b.ToSButton().IsActionButton()) return;
        
        Point mousePosition = Game1.getMousePosition();
        
        if (_colourWheel.Selected)
        {
            HsvColour colourBeforeChange = PickedColourHsv;
            HsvColour hsv = _colourWheel.GetColourAtScreenPoint(new Vector2(mousePosition.X, mousePosition.Y)).ToHsv();
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
            slider.leftClickHeld(mousePosition.X, mousePosition.Y);
        }
    }

    public override ClickableComponent getCurrentlySnappedComponent()
    {
        return base.getCurrentlySnappedComponent();
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
    }
    
    public override bool overrideSnappyMenuCursorMovementBan()
    {
        // return base.overrideSnappyMenuCursorMovementBan();
        return _colourWheel.Selected;
    }
    
    public override bool shouldClampGamePadCursor()
    {
        return base.shouldClampGamePadCursor();
    }
    
    public override void receiveKeyPress(Keys key)
    {
        if (ModEntry.ModHelper.Input.IsDown(SButton.Escape))
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
        }

        if (_hexInput.containsPoint(x, y))
        {
            _hexInput.Text = PickedColourRgb.ToHexString()[..6];
            _hexInput.SelectMe();
            _hexInput.caretTimer = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            Game1.playSound("dialogueCharacter");
        }
        else _hexInput.Selected = false;
        
        if (_randomHexButton.containsPoint(x, y))
        {
            _randomHexButton.scale = _randomHexButton.baseScale * 0.975f;
            Random rng = new Random();
            int r = rng.Next(0, 256);
            int g = rng.Next(0, 256);
            int b = rng.Next(0, 256);
            Color newColour = new Color(r, g, b, PickedColourRgb.ToXnaColor().A);
            SetColour(HsvColour.FromXnaColor(newColour));
            Game1.playSound("drumkit6");
        }
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
    }

    public override void performHoverAction(int x, int y)
    {
        if (Game1.options.SnappyMenus && !Game1.isAnyGamePadButtonBeingHeld())
        {
            if (_colourWheel.Selected) _colourWheel.Selected = false;
            foreach (var slider in _sliders.Values.Where(slider => slider.Selected))
            {
                slider.Selected = false;
            }
        }
        
        base.performHoverAction(x, y);
        _toggleAdvancedControls.tryHover(x, y);
        _togglePreviewBase.tryHover(x, y);
        _randomHexButton.tryHover(x, y);
    }
}