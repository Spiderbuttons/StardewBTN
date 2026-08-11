using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using FishPondDye.Components;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using FishPondDye.Config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using FishPondDye.Helpers;
using Microsoft.Xna.Framework.Graphics;

namespace FishPondDye
{
    internal sealed class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        internal static ModConfig Config { get; set; } = null!;
        internal static Harmony Harmony { get; set; } = null!;

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

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F2)
            {
                //
            }
            
            if (!Context.IsWorldReady)
                return;

            if (e.Button is SButton.F3)
            {
                if (Game1.activeClickableMenu is not null) Game1.activeClickableMenu = null;
                else
                {
                    Game1.activeClickableMenu = new ColourPicker(null);
                }
            }
        }
    }
}