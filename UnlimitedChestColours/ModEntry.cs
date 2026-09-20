using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpiderCore.Common.Colour;
using SpiderCore.Common.Shaders;
using SpiderCore.Common.Logging;
using SpiderCore.Common.Menus.ColourPicker;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewModdingAPI;
// ReSharper disable MemberCanBePrivate.Global

namespace UnlimitedChestColours
{
    internal sealed class ModEntry : Mod
    {
        internal static string UNIQUE_ID => Manifest.UniqueID;

        internal static IManifest Manifest { get; set; } = null!;
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static Harmony Harmony { get; set; } = null!;

        private static readonly List<Color> VanillaChestColours = [
        
            new(85, 85, 255), 
            new(119, 191, 255), 
            new(0, 170, 170), 
            new(0, 234, 175), 
            new(0, 170, 0), 
            new(159, 236, 0), 
            new(255, 234, 18), 
            new(255, 167, 18), 
            new(255, 105, 18), 
            new(255, 0, 0), 
            new(135, 0, 35), 
            new(255, 173, 199), 
            new(255, 117, 195), 
            new(172, 0, 198), 
            new(143, 0, 255), 
            new(89, 11, 142), 
            new(64, 64, 64), 
            new(100, 100, 100), 
            new(200, 200, 200), 
            new(254, 254, 254)
        ];

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;
            Manifest = ModManifest;
            ModHelper = helper;
            ShaderUtilities.Helper = ModHelper;

            Harmony = new Harmony(ModManifest.UniqueID);
            Harmony.PatchAll();
        }
        
        public static void OpenColourPicker(ItemGrabMenu menu)
        {
            if (menu.sourceItem is not Chest chest) return;
            
            menu.hoverText = "";
            List<Chest> previewChests = [
                new(true),
                new(true, "232"),
                new(true, "BigChest"),
                new(true, "BigStoneChest")
            ];
            
            var colourMenu = new ColourPickerMenu(OnColourPickerClosed, ChestPreview, previewObject: previewChests, colourableObject: chest, previousMenu: menu, allowAlpha: false, paletteSquaresPerRow: 10 , paletteRows: 3, paletteColours: VanillaChestColours);
            Color startingColour = chest.playerChoiceColor.Value;
            if (startingColour != Color.Black) colourMenu.SetColour(RgbColour.FromXnaColor(startingColour));
            colourMenu.ShowAdvancedControls();
            colourMenu.ShowPreview();
            Game1.activeClickableMenu = colourMenu;
        }

        private static void OnColourPickerClosed(ColourPickerMenu.CloseReason reason, RgbColour colour, object? colourableObject)
        {
            if (reason is not ColourPickerMenu.CloseReason.Confirmed || colourableObject is not Chest chest) return;
            
            Color finalColour = colour.ToXnaColor();
            
            // The game treats black as the player having not chosen any colour at all.
            if (finalColour.Equals(Color.Black)) finalColour = new Color(1, 1, 1, 255);
            chest.playerChoiceColor.Value = finalColour;
        }
        
        private static void ChestPreview(SpriteBatch b, Rectangle bounds, RgbColour colour, object? previewObject)
        {
            if (previewObject is not List<Chest> chests) return;
            
            float chestScale = Math.Min(bounds.Width / 64f, bounds.Height / 64f);
            Vector2 drawLocation = bounds.Center.ToVector2();

            for (var i = 0; i < chests.Count; i++)
            {
                var chest = chests[i];
                drawLocation.X = bounds.Center.X - 20f * chestScale + (i % 2) * 24f * chestScale;
                drawLocation.Y = bounds.Center.Y - 16f * chestScale - 2f * chestScale + (int)(i / 2f) * 28f * chestScale;
                drawChest(b, chest, drawLocation, chestScale, colour.ToXnaColor());
            }
        }

        // Once again I have to copy the vanilla draw code nearly wholesale because I can't change the scale otherwise...
        private static void drawChest(SpriteBatch b, Chest chest, Vector2 location, float scale, Color colour)
        {
            ParsedItemData chestData = ItemRegistry.GetData(chest.QualifiedItemId);
            if (chestData == null) return;
            
            int drawIndex = chest.QualifiedItemId switch 
            {
                "(BC)130" => 168,
                "(BC)BigChest" => 312,
                _ => chest.ParentSheetIndex
            };
            int overlayIndex = chest.startingLidFrame.Value + chest.QualifiedItemId switch 
            {
                "(BC)130" => 46,
                "(BC)BigChest" => 16,
                _ => 8
            };
            int coloredLidIndex = chest.startingLidFrame.Value + chest.QualifiedItemId switch 
            {
                "(BC)130" => 38,
                "(BC)BigChest" => 8,
                _ => 0
            };
            Rectangle drawRect = chestData.GetSourceRect(0, drawIndex);
            Rectangle lidRect = chestData.GetSourceRect(0, overlayIndex);
            Rectangle coloredLidRect = chestData.GetSourceRect(0, coloredLidIndex);
            Texture2D texture = chestData.GetTexture();
            var (x, y) = location;
                
            b.Draw(texture, new Vector2(x, y - 16 * scale), drawRect, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9f);
            b.Draw(texture, new Vector2(x, y - 16 * scale), coloredLidRect, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9f);
            b.Draw(texture, new Vector2(x, y + 4 * scale + scale), new Rectangle(0, drawIndex / 8 * 32 + 53, 16, 11), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.91f);
            b.Draw(texture, new Vector2(x, y - 16 * scale), lidRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.91f);
        }
    }
}