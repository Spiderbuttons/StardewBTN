using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiderCore.Common.Colour;
using SpiderCore.Common.Logging;
using SpiderCore.Common.Menus.ColourPicker;
using SpiderCore.Common.Shaders;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

// ReSharper disable MemberCanBePrivate.Global

namespace UnlimitedChestColours
{
    internal sealed class ModEntry : Mod
    {
        internal static string UNIQUE_ID => Manifest.UniqueID;

        internal static IManifest Manifest { get; set; } = null!;
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static Harmony Harmony { get; set; } = null!;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;
            Manifest = ModManifest;
            ModHelper = helper;
            ShaderUtilities.Helper = ModHelper;

            Harmony = new Harmony(ModManifest.UniqueID);
            Harmony.PatchAll();

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button is SButton.F2)
            {
                if (Game1.activeClickableMenu is not null) Game1.activeClickableMenu.exitThisMenu();
                else Game1.activeClickableMenu = new ColourPickerMenu(OnColourPickerClosed, ChestPreview, allowAlpha: false);
            }
        }

        private void OnColourPickerClosed(RgbColour colour, ColourPickerMenu.CloseReason reason)
        {
            Log.Info($"Colour picker closed with colour {colour} and reason {reason}");
        }
        
        private void ChestPreview(SpriteBatch b, Rectangle bounds, RgbColour colour, object? previewObject)
        {
            b.Draw(Game1.staminaRect, bounds, colour.ToXnaColor());
        }
    }
}