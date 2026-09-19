using System;
using System.Linq;
using SpiderCore.Common.Colour;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;

namespace SpiderCore.Common.Menus.ColourPicker
{
    public partial class ColourPickerMenu
    {
        private bool ShouldUseMouseInputFunction()
        {
            bool isAnySliderSelected = _sliders.Any(slider => slider.Value.Selected);
            return !Game1.options.SnappyMenus || (!_colourWheel.Selected && !isAnySliderSelected);
        }
    
        public override void snapCursorToCurrentSnappedComponent()
        {
            if (currentlySnappedComponent is null) return;
            int currentID = currentlySnappedComponent.myID;
        
            if (currentID == CC_SELECTION_CIRCLE)
            {
                Game1.setMousePosition(_selectionCircle.bounds.Center, true);
                return;
            }
        
            if (currentID >= CC_SLIDERS_START && currentID <= CC_SLIDERS_START + _sliders.Count - 1)
            {
                var slider = _sliders.ElementAt(currentID - CC_SLIDERS_START).Value;
                Point sliderGrabber = slider.GetGrabberCenter().ToPoint() + new Point(1, 0);
                Game1.setMousePosition(sliderGrabber, true);
                return;
            }
        
            base.snapCursorToCurrentSnappedComponent();
        }

        public override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            if (oldID == CC_TOGGLE_ADVANCED && direction is 1)
            {
                if (!_showingAdvancedControls) return;
                setCurrentlySnappedComponentTo(CC_SLIDERS_START + _sliders.Count - 1);
            }

            if (oldID == CC_CONFIRM && direction is 1)
            {
                if (!_showingAdvancedControls) return;
                setCurrentlySnappedComponentTo(CC_SLIDERS_START);
            }

            if (oldID == CC_SELECTION_CIRCLE && direction is 1)
            {
                if (!_showingAdvancedControls) setCurrentlySnappedComponentTo(CC_TOGGLE_ADVANCED);
                else
                {
                    Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
                    bool selectionCircleIsInBottomQuarter = point.Y > 0.5f;
                
                    if (selectionCircleIsInBottomQuarter && _selectionCircle.bounds.Center.X < _toggleAdvancedControls.bounds.Left)
                    {
                        setCurrentlySnappedComponentTo(CC_TOGGLE_ADVANCED);
                    }
                    else
                    {
                        float closestSliderDistance = float.MaxValue;
                        int closestSliderID = -1;
                        foreach (var slider in _sliders.Values)
                        {
                            float distance = Vector2.Distance(_selectionCircle.bounds.Center.ToVector2(),
                                slider.Bar.Bounds.Center.ToVector2());
                            if (!(distance < closestSliderDistance)) continue;

                            closestSliderDistance = distance;
                            closestSliderID = slider.myID;
                        }
                        setCurrentlySnappedComponentTo(closestSliderID);
                    }
                }
            }
        
            base.customSnapBehavior(direction, oldRegion, oldID);
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = _selectionCircle;
            snapCursorToCurrentSnappedComponent();
        }

        public override void gamePadButtonHeld(Buttons b)
        {
            if (!b.ToSButton().IsActionButton()) return;
        
            Point mousePosition = Game1.getMousePosition();
        
            if (_colourWheel.Selected)
            {
                _colourWheel.Selected = true;
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
        
            if (!_showingAdvancedControls || _rightSectionOffset.X < width * 0.9f) return;

            foreach (var (_, slider) in _sliders)
            {
                slider.leftClickHeld(mousePosition.X, mousePosition.Y);
            }
        }
    
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return !ShouldUseMouseInputFunction();
        }
    
        public override void receiveKeyPress(Keys key)
        {
            if (key is Keys.Escape)
            {
                _hexInput.Selected = false;
            }
            
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
            {
                _onClose?.Invoke(CloseReason.Cancelled, PickedColourRgb, _colourableObject);
                _onClose = null;
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
            UpdateComponentIDs();
        }

        public override void leftClickHeld(int x, int y)
        {
            if (!ShouldUseMouseInputFunction()) return;
        
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
        
            if (!_showingAdvancedControls || _rightSectionOffset.X < width * 0.9f) return;

            foreach (var (_, slider) in _sliders)
            {
                slider.leftClickHeld(x, y);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!ShouldUseMouseInputFunction()) return;
        
            _colourWheel.Selected = _colourWheel.containsPoint(x, y) || _selectionCircle.containsPoint(x, y);
            if (_colourWheel.Selected)
            {
                if (_value is 0) _value = 100M;
                Game1.playSound("button_tap");
                timeUntilNextSound = 50f;
            }
        
            foreach (var square in _palette.Where(square => square.containsPoint(x, y)))
            {
                if (square.IsAddSquare)
                {
                    AddColourToPalette(PickedColourHsv);
                    Game1.playSound("shwip");
                    break;
                }

                HsvColour storedColour = square.StoredColour.HasValue
                    ? HsvColour.FromXnaColor(square.StoredColour.Value)
                    : HsvColour.FromXnaColor(Color.White);
                if (!_allowAlpha) storedColour = new HsvColour(storedColour.Hue, storedColour.Saturation, storedColour.Value);
                SetColour(storedColour);

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
                _showingPreview = !_showingPreview;
                Game1.playSound("drumkit6");
            }
        
            if (_cancelButton.containsPoint(x, y))
            {
                _cancelButton.scale = _cancelButton.baseScale * 0.975f;
                _onClose?.Invoke(CloseReason.Cancelled, PickedColourRgb, _colourableObject);
                _onClose = null;
                exitThisMenu();
            }
        
            if (_confirmButton.containsPoint(x, y))
            {
                _confirmButton.scale = _confirmButton.baseScale * 0.975f;
                _onClose?.Invoke(CloseReason.Confirmed, PickedColourRgb, _colourableObject);
                _onClose = null;
                exitThisMenu();
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
                _hexInput.Update();
                _hexInput.caretTimer = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                if (!Game1.options.SnappyMenus) Game1.playSound("dialogueCharacter");
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
            base.performHoverAction(x, y);
            _toggleAdvancedControls.tryHover(x, y);
            _togglePreviewBase.tryHover(x, y);
            _cancelButton.tryHover(x, y, maxScaleIncrease: 0.1f / 4f);
            _confirmButton.tryHover(x, y, maxScaleIncrease: 0.1f / 4f);
            _randomHexButton.tryHover(x, y, maxScaleIncrease: 0.2f);
        }
    }
}