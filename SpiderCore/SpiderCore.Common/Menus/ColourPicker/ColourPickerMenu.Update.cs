using System;
using System.Linq;
using SpiderCore.Common.Colour;
using SpiderCore.Common.Menus.ColourPicker.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace SpiderCore.Common.Menus.ColourPicker
{
    public partial class ColourPickerMenu
    {
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            width = Game1.uiViewport.Width / 4;
        
            Rectangle colourWheelBounds = GetColourWheelBounds();
            _colourWheel.CenterPoint = new Vector2(colourWheelBounds.X + colourWheelBounds.Width / 2f, colourWheelBounds.Y + colourWheelBounds.Height / 2f);
            _colourWheel.Width = _colourWheel.Height = colourWheelBounds.Width;
        
            UpdateSelectionCircle();
        
            PositionPaletteSquares();

            Rectangle pickedColourBounds = GetPickedColourBounds();
            PickedColourSlider.UpdateBarBounds(pickedColourBounds);

            float buttonScale = GetAdvancedButtonScale();
            _toggleAdvancedControls.baseScale = _toggleAdvancedControls.scale = buttonScale;
            _togglePreviewBase.baseScale = _togglePreviewBase.scale = buttonScale;
            _togglePreviewIcon.baseScale = _togglePreviewIcon.scale = buttonScale * 0.69f;
            _toggleAdvancedControls.bounds = GetAdvancedButtonBounds();
        
            Rectangle previewButtonBounds = GetPreviewButtonBounds();
            _togglePreviewBase.bounds = previewButtonBounds;
        
            Rectangle previewIconBounds = new Rectangle(
                x: previewButtonBounds.X + (int)(previewButtonBounds.Width * 0.5f) - (int)(_togglePreviewIcon.sourceRect.Width * buttonScale * 0.75f) / 2 + 1,
                y: previewButtonBounds.Y + (int)(previewButtonBounds.Height * 0.5f) - (int)(_togglePreviewIcon.sourceRect.Height * buttonScale * 0.75f) / 2 + 1,
                width: (int)(_togglePreviewIcon.sourceRect.Width * buttonScale * 0.75f),
                height: (int)(_togglePreviewIcon.sourceRect.Height * buttonScale * 0.75f)
            );
            _togglePreviewIcon.bounds = previewIconBounds;
        
            _cancelButton.baseScale = _cancelButton.scale = buttonScale / 4f;
            _cancelButton.bounds = GetCancelButtonBounds();
        
            _confirmButton.baseScale = _confirmButton.scale = buttonScale / 4f;
            _confirmButton.bounds = GetConfirmButtonBounds();

            _rightSectionOffset.X = _showingAdvancedControls ? width : 0;
            _leftSectionOffset.X = _showingPreview ? -width : 0;
        
            // This'll make the menu fit all our stuff in it, but only just. Nice n cozy size.
            (int squareSize, int gap) = GetPaletteSquareSizeAndGap();
            float totalHeight = _colourWheel.Height + gap * 2 + squareSize * 3 + gap * 3;
            height = (int)totalHeight;
        
            UpdateSliderPositions();
            UpdateHexInputPosition();
        }
    
        public override void update(GameTime time)
        {
            base.update(time);
            UpdateComponentIDs();
        
            UpdateSliderColours();
            UpdateSelectionCircle();
        
            float targetRightOffsetX = _showingAdvancedControls ? width : 0f;
            _rightSectionOffset.X = MathHelper.Lerp(_rightSectionOffset.X, targetRightOffsetX, 0.15f);
            if (Math.Abs(_rightSectionOffset.X - targetRightOffsetX) < 0.5f)
            {
                _rightSectionOffset.X = targetRightOffsetX;
            }
        
            float targetLeftOffsetX = _showingPreview ? -width : 0f;
            _leftSectionOffset.X = MathHelper.Lerp(_leftSectionOffset.X, targetLeftOffsetX, 0.15f);
            if (Math.Abs(_leftSectionOffset.X - targetLeftOffsetX) < 0.5f)
            {
                _leftSectionOffset.X = targetLeftOffsetX;
            }
        
            if (Math.Abs(_rightSectionOffset.X - targetRightOffsetX) > float.Epsilon)
            {
                UpdateSliderPositions();
                UpdateHexInputPosition();
            }

            if (_colourWheel.Selected && Game1.isGamePadThumbstickInMotion())
            {
                ClampGamePadCursorToColourWheel();
            }
            ColourSlider? selectedSlider = _sliders.Values.FirstOrDefault(s => s.Selected);
            if (selectedSlider is not null && Game1.isGamePadThumbstickInMotion())
            {
                ClampGamePadCursorToSlider(selectedSlider);
            }
        }

        private void UpdateSelectionCircle()
        {
            Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
            Vector2 position = new Vector2(
                x: _colourWheel.CenterPoint.X + point.X * _colourWheel.Width / 2f,
                y: _colourWheel.CenterPoint.Y + point.Y * _colourWheel.Height / 2f
            );
        
            _selectionCircle.setPosition(
                (int)position.X - _selectionCircle.bounds.Width / 2,
                (int)position.Y - _selectionCircle.bounds.Height / 2
            );
        }
    
        private void ClampGamePadCursorToColourWheel()
        {
            GamePadState state = Game1.input.GetGamePadState();
            Vector2 stickMovement = new Vector2(
                x: state.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier,
                y: -state.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier
            );
            Vector2 currentPosition = Game1.getMousePosition().ToVector2();
            Vector2 nextPosition = currentPosition + stickMovement;
        
            if (!_colourWheel.containsPoint((int)nextPosition.X, (int)nextPosition.Y))
            {
                Vector2 direction = nextPosition - _colourWheel.CenterPoint;
                direction.Normalize();
                Vector2 clampedPosition = _colourWheel.CenterPoint + direction * (_colourWheel.Width / 2f);
                Game1.setMousePosition((int)clampedPosition.X, (int)clampedPosition.Y);
            }
        }

        private static void ClampGamePadCursorToSlider(ColourSlider slider)
        {
            GamePadState state = Game1.input.GetGamePadState();
            Point stickMovement = new Point(
                x: (int)(state.ThumbSticks.Left.X * Game1.thumbstickToMouseModifier * 0.175f),
                y: (int)(-state.ThumbSticks.Left.Y * Game1.thumbstickToMouseModifier * 0.175f)
            );
            Point currentPosition = Game1.getMousePositionRaw();
            Point nextPosition = currentPosition + stickMovement;
        
            Rectangle rawSliderBounds = new Rectangle(
                x: (int)(slider.Bar.Bounds.X * Game1.options.uiScale),
                y: (int)(slider.Bar.Bounds.Y * Game1.options.uiScale),
                width: (int)(slider.Bar.Bounds.Width * Game1.options.uiScale),
                height: (int)(slider.Bar.Bounds.Height * Game1.options.uiScale)
            );
        
            int grabberHalfWidth = (int)(slider.GetGrabberBounds().Width / 8f * Game1.options.uiScale);
            if (nextPosition.X < rawSliderBounds.Left - grabberHalfWidth || nextPosition.X > rawSliderBounds.Right + grabberHalfWidth)
            {
                int clampedX = Math.Clamp(nextPosition.X, rawSliderBounds.Left - grabberHalfWidth, rawSliderBounds.Right + grabberHalfWidth);
                Game1.setMousePositionRaw(
                    x: clampedX,
                    y: rawSliderBounds.Center.Y + 1
                );
            } else {
                Game1.setMousePositionRaw(
                    x: nextPosition.X,
                    y: rawSliderBounds.Center.Y + 1
                );
            }
        }

        private void UpdateComponentIDs()
        {
            Vector2 point = ColourWheel.HsvToPoint(PickedColourHsv);
            bool selectionCircleIsInBottomQuarter = point.Y > 0.5f;
            bool selectionCircleIsInTopQuarter = point.Y < -0.5f;
            switch (point.X)
            {
                case < -0.35f:
                case < 0 when !selectionCircleIsInBottomQuarter:
                    _selectionCircle.downNeighborID = CC_TOGGLE_PREVIEW;
                    break;
                case > 0.35f:
                case >= 0 when !selectionCircleIsInBottomQuarter:
                    _selectionCircle.downNeighborID = CC_TOGGLE_ADVANCED;
                    break;
                default:
                    if (_selectionCircle.downNeighborID is >= CC_PALETTE_START and < CC_PALETTE_START + _paletteSquaresPerRow)
                    {
                        break;
                    }
                
                    float closestDistance = float.MaxValue;
                    int closestIndex = -1;
                    for (var i = 0; i < _paletteSquaresPerRow; i++)
                    {
                        var square = _palette[i];
                        float distance = Vector2.Distance(_selectionCircle.bounds.Center.ToVector2(), square.bounds.Center.ToVector2());
                        if (!(distance < closestDistance)) continue;
                    
                        closestDistance = distance;
                        closestIndex = i;
                    }
                    _selectionCircle.downNeighborID = CC_PALETTE_START + closestIndex;
                    break;
            }
        
            _selectionCircle.upNeighborID = point.X < 0 ? CC_CANCEL : CC_CONFIRM;
            _selectionCircle.leftNeighborID = point.Y > 0 ? CC_TOGGLE_PREVIEW : CC_CANCEL;
            _selectionCircle.rightNeighborID = point.Y > 0 ? CC_TOGGLE_ADVANCED : CC_CONFIRM;

            if (selectionCircleIsInBottomQuarter)
            {
                _toggleAdvancedControls.leftNeighborID = CC_SELECTION_CIRCLE;
                _togglePreviewBase.rightNeighborID = CC_SELECTION_CIRCLE;
            } else
            {
                _toggleAdvancedControls.leftNeighborID = CC_TOGGLE_PREVIEW;
                _togglePreviewBase.rightNeighborID = CC_TOGGLE_ADVANCED;
            }

            if (selectionCircleIsInTopQuarter)
            {
                _cancelButton.rightNeighborID = CC_SELECTION_CIRCLE;
                _confirmButton.leftNeighborID = CC_SELECTION_CIRCLE;
            } else 
            {
                _cancelButton.rightNeighborID = CC_CONFIRM;
                _confirmButton.leftNeighborID = CC_CANCEL;
            }
        }

        private void UpdateHexInputPosition()
        {
            Rectangle inputBounds = GetHexInputBounds();
            _hexInput.X = inputBounds.X;
            _hexInput.Y = inputBounds.Y;
            _hexInput.Width = inputBounds.Width;
            _hexInput.Height = inputBounds.Height;
            _hexInputCC.bounds = inputBounds;
        
            _randomHexButton.bounds = GetRandomButtonBounds();
            _randomHexButton.baseScale = _randomHexButton.scale = GetRandomButtonScale();
        }

        private void UpdateSliderPositions()
        {
            for (var i = 0; i < _sliders.Count; i++)
            {
                string sliderKey = i switch
                {
                    0 => "Red",
                    1 => "Green",
                    2 => "Blue",
                    3 => "Hue",
                    4 => "Saturation",
                    5 => "Value",
                    _ => _allowAlpha ? "Alpha" : throw new InvalidOperationException("Alpha slider is disabled but being updated anyway."),
                };
                ColourSlider slider = _sliders[sliderKey];
            
                Rectangle newBarBounds = i switch
                {
                    < 3 => GetRgbSliderBounds(i),
                    < 6 => GetHsvSliderBounds(i - 3),
                    _ => GetAlphaSliderBounds(),
                };
                slider.UpdateBarBounds(newBarBounds);
            
                Rectangle newInputBounds = i switch
                {
                    < 3 => GetRgbInputBounds(i),
                    < 6 => GetHsvInputBounds(i - 3),
                    _ => GetAlphaInputBounds(),
                };
                slider.UpdateInputBounds(newInputBounds);
                slider.Input?.UpdateButtonPositions();
            }
        }

        private void UpdateSliderColours()
        {
            Color pickedColour = PickedColourHsv.ToXnaColor() * (float)(_alpha / 100);
            PickedColourSlider.UpdateColours(pickedColour, pickedColour);
        
            Color minRed = new Color(0, (byte)GetGreen(), (byte)GetBlue());
            Color maxRed = new Color(255, (byte)GetGreen(), (byte)GetBlue());
            Color minGreen = new Color((byte)GetRed(), 0, (byte)GetBlue());
            Color maxGreen = new Color((byte)GetRed(), 255, (byte)GetBlue());
            Color minBlue = new Color((byte)GetRed(), (byte)GetGreen(), 0);
            Color maxBlue = new Color((byte)GetRed(), (byte)GetGreen(), 255);
            _sliders["Red"].UpdateColours(minRed, maxRed);
            _sliders["Green"].UpdateColours(minGreen, maxGreen);
            _sliders["Blue"].UpdateColours(minBlue, maxBlue);
        
            Color minSaturation = new HsvColour(_hue, 0, _value).ToXnaColor();
            Color maxSaturation = new HsvColour(_hue, 100, _value).ToXnaColor();
            Color minValue = new HsvColour(_hue, _saturation, 0).ToXnaColor();
            Color maxValue = new HsvColour(_hue, _saturation, 100).ToXnaColor();
            _sliders["Saturation"].UpdateColours(minSaturation, maxSaturation);
            _sliders["Value"].UpdateColours(minValue, maxValue);
            if (_allowAlpha)
            {
                Color minAlpha = new HsvColour(_hue, _saturation, _value, 0).ToXnaColor();
                Color maxAlpha = new HsvColour(_hue, _saturation, _value, 100).ToXnaColor();
                _sliders["Alpha"].UpdateColours(minAlpha, maxAlpha);
            }
        }
    }
}