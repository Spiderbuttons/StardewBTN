using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using FishPondDye.APIs;
using HarmonyLib;
using Microsoft.Xna.Framework;
using FishPondDye.Config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu;
using FishPondDye.Menus.ColourPickerMenu.Components;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FishPondDye
{
    internal sealed class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        internal static ModConfig Config { get; set; } = null!;
        internal static Harmony Harmony { get; set; } = null!;

        // This one is only used for drawing the preview in the colour picker menu.
        private static FishPond? DummyPond;

        public override void Entry(IModHelper helper)
        {
            i18n.Init(helper.Translation);
            ModHelper = helper;
            ModMonitor = Monitor;
            Config = helper.ReadConfig<ModConfig>();
            Harmony = new Harmony(ModManifest.UniqueID);

            Harmony.PatchAll();

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            ShaderHelper.WatchShader("colourWheel", effect =>
            {
                ColourWheel.ColourWheelEffect = effect;
            });
            ShaderHelper.WatchShader("gradientBar", effect =>
            {
                GradientBar.GradientBarEffect = effect;
            });
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null) Config.SetupConfig(configMenu, ModManifest, Helper);
        }
        
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/Node"))
            {
                e.LoadFromModFile<Texture2D>("assets/node.png", AssetLoadPriority.Exclusive);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/FloppyButton"))
            {
                e.LoadFromModFile<Texture2D>("assets/floppy_button.png", AssetLoadPriority.Exclusive);
            }
            
            if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/ResetButton"))
            {
                e.LoadFromModFile<Texture2D>("assets/reset_button.png", AssetLoadPriority.Exclusive);
            }
        }

        private static void DrawPondPreview(SpriteBatch b, Rectangle bounds, RgbColour colour)
        {
            DummyPond ??= new FishPond();
            if (DummyPond.texture?.Value is not { } texture)
                return;
            
            Color xnaColour = colour.ToXnaColor();
            float alpha = 0.5f;
            if (xnaColour == Color.White)
            {
                xnaColour = new Color(254, 254, 254);
                // The game has special handling for exactly white to change it to the current location's water colour.
                // So we fudge it a little bit here so the player is still able to actually see white if they want white. 
            }
            
            float scale = Math.Min(bounds.Width / 80f, bounds.Height / 80f);
            float waterScale = scale / 4f;

            Vector2 basePosition = new Vector2(bounds.Left - 1, bounds.Center.Y) + new Vector2(0, 80) * scale / 2f;
            b.Draw(
                texture: Game1.mouseCursors,
                position: basePosition,
                sourceRectangle: Building.leftShadow,
                color: Color.White * DummyPond.alpha,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: new Vector2(scale, scale / 2f),
                effects: SpriteEffects.None,
                layerDepth: 1E-05f);
            
            for (int x = 1; x < 4; x++)
            {
                b.Draw(
                    texture: Game1.mouseCursors,
                    position: basePosition + new Vector2(x * 64, 0f) * scale / 4f,
                    sourceRectangle: Building.middleShadow,
                    color: Color.White * DummyPond.alpha,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: new Vector2(scale, scale / 2f),
                    effects: SpriteEffects.None,
                    layerDepth: 1E-05f);
            }
            
            b.Draw(
                texture: Game1.mouseCursors,
                position: basePosition + new Vector2(4 * 64, 0f) * scale / 4f,
                sourceRectangle: Building.rightShadow,
                color: Color.White * DummyPond.alpha,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: new Vector2(scale, scale / 2f),
                effects: SpriteEffects.None,
                layerDepth: 1E-05f
            );
            
            b.Draw(
                texture: DummyPond.texture.Value,
                position: new Vector2(x: bounds.Center.X, y: bounds.Center.Y),
                sourceRectangle: new Rectangle(
                    x: 0,
                    y: 80,
                    width: 80,
                    height: 80),
                color: xnaColour * DummyPond.alpha,
                rotation: 0f,
                origin: new Vector2(x: 40f, y: 40f),
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0.75f
            );
            
            for (int yWater = 0; yWater < 5; yWater++)
            {
                for (int xWater = 0; xWater < 4; xWater++)
                {
                    bool isLastRow = yWater == 4;
                    bool isFirstRow = yWater == 0;

                    Vector2 position = new Vector2(
                        x: bounds.X + xWater * 64 * waterScale + 32 * waterScale,
                        y: bounds.Y + 44 * waterScale + (yWater + 1) * (64 * waterScale) - (int)(Game1.currentLocation.waterPosition + 32) * waterScale
                    );
                    if (!isLastRow)
                    {
                        position.Y = bounds.Y + 44 * waterScale + yWater * (64 * waterScale) + 32 * waterScale - (int)(!isFirstRow ? Game1.currentLocation.waterPosition : 0f) * waterScale;
                    }
                    
                    Rectangle sourceRect = new Rectangle(
                        x: Game1.currentLocation.waterAnimationIndex * 64,
                        y: 2064 + ((xWater + yWater) % 2 != 0
                               ? !Game1.currentLocation.waterTileFlip
                                   ? 128
                                   : 0
                               : Game1.currentLocation.waterTileFlip
                                   ? 128
                                   : 0) +
                           (isFirstRow ? (int)Game1.currentLocation.waterPosition : 0),
                        width: 64,
                        height: isLastRow ? 32 + (int)Game1.currentLocation.waterPosition - 5 : 64 + (isFirstRow ? (int)(0f - Game1.currentLocation.waterPosition) : 0)
                    );
                    
                    b.Draw(
                        texture: Game1.mouseCursors,
                        position: position,
                        sourceRectangle: sourceRect,
                        color: xnaColour * alpha,
                        rotation: 0f,
                        origin: new Vector2(0, 0),
                        scale: waterScale,
                        effects: SpriteEffects.None,
                        layerDepth: 0.8f);
                }
            }
            
            b.Draw(
                texture: DummyPond.texture.Value,
                position: new Vector2(x: bounds.Center.X, y: bounds.Center.Y),
                sourceRectangle: new Rectangle(x: 0, y: 0, width: 80, height: 80),
                color: DummyPond.color * DummyPond.alpha,
                rotation: 0f,
                origin: new Vector2(x: 40f, y: 40f),
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0.9f
            );
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F2)
            {
                const string RED = "\x1b[91m";
                const string GREEN = "\x1b[92m";
                const string RESET = "\x1b[0m";

                Random rng = new Random();
                {
                    Color original = new Color((byte)rng.Next(0, 256), (byte)rng.Next(0, 256), (byte)rng.Next(0, 256));
                    RgbColour rgb = RgbColour.FromXnaColor(original);
                    HsvColour hsv = rgb.ToHsv();
                    RgbColour roundtrip = RgbColour.FromHsv(hsv);
                    Color final = roundtrip.ToXnaColor();
                    bool redMatch = original.R == final.R;
                    bool greenMatch = original.G == final.G;
                    bool blueMatch = original.B == final.B;
                    // Log.Debug($"\nRGB: {rgb} -> HSV: {hsv} -> RGB: {roundtrip}");
                    Log.Info($"R: {(redMatch ? GREEN : RED)}{final.R} {GREEN}({original.R}){RESET} " +
                             $"G: {(greenMatch ? GREEN : RED)}{final.G} {GREEN}({original.G}){RESET} " +
                             $"B: {(blueMatch ? GREEN : RED)}{final.B} {GREEN}({original.B}){RESET}");
                }
            }
            
            if (!Context.IsWorldReady)
                return;

            if (e.Button is SButton.F3)
            {
                if (Game1.activeClickableMenu is not null) Game1.activeClickableMenu.exitThisMenu();
                else
                {
                    Game1.activeClickableMenu = new ColourPickerMenu(null, DrawPondPreview);
                }
            }
        }
    }
}