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

        private unsafe void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F2)
            {
                Dictionary<string, double> timesToComputer = new()
                {
                    { "Unsafe", 0 },
                    { "UnsafeReverse", 0 },
                    { "BitConverter", 0 },
                    { "BlockCopy", 0 },
                    { "MemoryMarshal", 0 },
                    { "AsBytes", 0}
                };

                for (int i = 0; i < 100_000; i++)
                {
                    Stopwatch watch = new();
                    watch.Start();
                    unsafe
                    {
                        ulong[] data =
                        [
                            0x00000000_FFFFFFFFul, 0xFFFFFFFF_00000000ul,
                            0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul,
                            0xFF0000FF_00FFFF00ul, 0x00FFFF00_FF0000FFul,
                            0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul
                        ];
                        fixed (ulong* topRows = data)
                        {
                            byte[] pixels = new byte[64];
                            Marshal.Copy((IntPtr)topRows, pixels, 0, 64);
                        }
                    }
                    watch.Stop();
                    timesToComputer["Unsafe"] += watch.Elapsed.TotalMilliseconds;

                    watch.Reset();
                    watch.Start();
                    byte[] output2 =
                        BitConverter.GetBytes(0xFF0000FF_00FFFF00ul)
                            .Concat(BitConverter.GetBytes(0x00FFFF00_FF0000FFul))
                            .Concat(BitConverter.GetBytes(0x00FFFF00_FF0000FFul))
                            .Concat(BitConverter.GetBytes(0xFF0000FF_00FFFF00ul))
                            .ToArray();
                    watch.Stop();
                    timesToComputer["BitConverter"] += watch.Elapsed.TotalMilliseconds;

                    watch.Reset();
                    watch.Start();
                    byte[] output3 = new byte[32];
                    ulong[] longs =
                        [0xFF0000FF_00FFFF00ul, 0x00FFFF00_FF0000FFul, 0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul];
                    Buffer.BlockCopy(longs, 0, output3, 0, 16);
                    watch.Stop();
                    timesToComputer["BlockCopy"] += watch.Elapsed.TotalMilliseconds;

                    watch.Reset();
                    watch.Start();
                    ulong[] longs2 = [0xFF0000FF_00FFFF00ul, 0x00FFFF00_FF0000FFul, 0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul];
                    var output4 = MemoryMarshal.Cast<ulong, byte>(longs2).ToArray();
                    watch.Stop();
                    timesToComputer["MemoryMarshal"] += watch.Elapsed.TotalMilliseconds;
                    
                    watch.Reset();
                    watch.Start();
                    ulong[] longs3 =
                        [0xFF0000FF_00FFFF00ul, 0x00FFFF00_FF0000FFul, 0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul];
                    var output5 = MemoryMarshal.AsBytes(longs3).ToArray();
                    watch.Stop();
                    timesToComputer["AsBytes"] += watch.Elapsed.TotalMilliseconds;

                    watch.Reset();
                    watch.Start();
                    unsafe
                    {
                        ulong[] data =
                        [
                            0x00000000_FFFFFFFFul, 0xFFFFFFFF_00000000ul,
                            0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul,
                            0xFF0000FF_00FFFF00ul, 0x00FFFF00_FF0000FFul,
                            0x00FFFF00_FF0000FFul, 0xFF0000FF_00FFFF00ul
                        ];
                        data = data.AsEnumerable().Reverse().ToArray();
                        fixed (ulong* topRows = data)
                        {
                            byte[] pixels = new byte[64];
                            Marshal.Copy((IntPtr)topRows, pixels, 0, 64);
                        }
                    }
                    watch.Stop();
                    timesToComputer["UnsafeReverse"] += watch.Elapsed.TotalMilliseconds;
                }
                Log.Alert($"Average times over 100_000 iterations:\n{string.Join("\n", timesToComputer.Select(kv => $"\t{kv.Key}: {(kv.Value * 1000000f) / 100_000:0.00} ns"))}");
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