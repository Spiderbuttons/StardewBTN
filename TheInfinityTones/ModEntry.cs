using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu;
using TheInfinityTones.Menus.ColourPickerMenu.Components;

namespace TheInfinityTones
{
    internal sealed class ModEntry : Mod
    {
        public static Effect BlurEffect
        {
            get
            {
                if (field is not null) return field;
            
                byte[] stream = File.ReadAllBytes(Path.Combine(ModHelper.DirectoryPath, "assets", "shaders", "blur.mgfx"));
                field = new Effect(Game1.graphics.GraphicsDevice, stream);
                return field;
            }
            set;
        }
        
        internal static IManifest Manifest { get; set; } = null!;
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        internal static Harmony Harmony { get; set; } = null!;
        
        public static ConditionalWeakTable<FarmerRenderer, Farmer> FarmerRendererToFarmerMap = new();

        public static SkinTone? StoredSkinTone;
        public static bool? StoredPaletteToggle;
        public static bool? StoredDarkSkinToggle;

        public override void Entry(IModHelper helper)
        {
            i18n.Init(helper.Translation);
            Manifest = ModManifest;
            ModHelper = helper;
            ModMonitor = Monitor;
            Harmony = new Harmony(ModManifest.UniqueID);
            
            Harmony.PatchAll();

            Harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.ResetGameStateOnTitleScreen)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Game1_ResetGameStateOnTitleScreen_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer._SwapColor)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(FarmerRenderer_SwapColor_Prefix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.createdNewCharacter)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(TitleMenu_createdNewCharacter_Prefix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(LoadGameMenu), nameof(LoadGameMenu.addSaveFiles)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(LoadGameMenu_addSaveFiles_Postfix))
            );

            Helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            
            ShaderHelper.WatchShader("blur", shader =>
            {
                BlurEffect = shader;
            });
        }
        
        private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(asset => asset.IsEquivalentTo("Characters/Farmer/skinColors")))
            {
                SkinTone.VanillaSkinTones = null!;
            }
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            //
        }
        
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button is SButton.F2)
            {

            }
            
            if (!Context.IsWorldReady)
                return;
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            FarmerRendererToFarmerMap.Clear();
            FarmerRendererToFarmerMap.Add(Game1.player.FarmerRenderer, Game1.player);
            Game1.player.FarmerRenderer.MarkSpriteDirty();
        }

        private static void TitleMenu_createdNewCharacter_Prefix()
        {
            if (StoredSkinTone is null || Game1.player is null) return;
            
            Game1.player.modData[$"{Manifest.UniqueID}/SkinTone"] = StoredSkinTone.ToString();
            Game1.player.modData[$"{Manifest.UniqueID}/DarkSkin"] = StoredPaletteToggle?.ToString() ?? "false";
            
            StoredSkinTone = null;
            StoredPaletteToggle = null;
            StoredDarkSkinToggle = null;
        }

        private static void LoadGameMenu_addSaveFiles_Postfix(LoadGameMenu __instance, List<Farmer> files)
        {
            FarmerRendererToFarmerMap.Clear();
            foreach (var farmer in files)
            {
                FarmerRendererToFarmerMap.Add(farmer.FarmerRenderer, farmer);
            }
        }
        
        private static void Game1_ResetGameStateOnTitleScreen_Postfix()
        {
            StoredSkinTone = null;
            StoredPaletteToggle = null;
            StoredDarkSkinToggle = null;
        }

        private static void FarmerRenderer_SwapColor_Prefix(FarmerRenderer __instance, string texture_name,
            Color[] pixels, int color_index, ref Color color)
        {
            if (color_index is < 256 or > 262) return;
            if (StoredSkinTone is not null)
            {

                color = color_index switch
                {
                    256 or 260 => GetSkinColor(0),
                    257 or 261 => GetSkinColor(1),
                    258 or 262 => GetSkinColor(2),
                    _ => color
                };
                return;
            }

            if (FarmerRendererToFarmerMap.TryGetValue(__instance, out var farmer))
            {
                SkinTone skinTone = GetSkinToneFromFarmer(farmer);
                color = color_index switch
                {
                    256 or 260 => skinTone.Darkest,
                    257 or 261 => skinTone.Medium,
                    258 or 262 => skinTone.Lightest,
                    _ => color
                };
            }
        }

        private static SkinTone GetSkinToneFromFarmer(Farmer who)
        {
            if (!who.modData.TryGetValue($"{Manifest.UniqueID}/SkinTone", out var skinToneString))
            {
                return SkinTone.VanillaSkinTones.ElementAtOrDefault(who.skin.Value);
            }
            try
            {
                return SkinTone.FromString(skinToneString);
            }
            catch (Exception ex)
            {
                ModMonitor.Log($"Failed to parse skin tone from farmer mod data: {ex}", LogLevel.Error);
                return SkinTone.VanillaSkinTones.ElementAtOrDefault(who.skin.Value);
            }
        }

        private static Color GetSkinColor(int column)
        {
            return column switch
            {
                0 => StoredSkinTone?.Darkest ?? SkinTone.VanillaSkinTones[0].Darkest,
                1 => StoredSkinTone?.Medium ?? SkinTone.VanillaSkinTones[0].Medium,
                2 => StoredSkinTone?.Lightest ?? SkinTone.VanillaSkinTones[0].Lightest,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be 0, 1, or 2.")
            };
        }
        
        private static CharacterCustomization? _storedCustomizationMenu;
        
        public static void RestoreCustomizationMenu()
        {
            if (_storedCustomizationMenu is not null)
            {
                TitleMenu.subMenu = _storedCustomizationMenu;
                _storedCustomizationMenu.RemoveDependency();
                _storedCustomizationMenu.ResetComponents();
                _storedCustomizationMenu.populateClickableComponentList();
                _storedCustomizationMenu = null;
            }
        }

        public static void StoreCustomizationMenu()
        {
            if (TitleMenu.subMenu is CharacterCustomization customization)
            {
                _storedCustomizationMenu = customization;
                _storedCustomizationMenu.AddDependency();
            }
        }
        
        public static CharacterCustomization? GetStoredCustomizationMenu()
        {
            return _storedCustomizationMenu;
        }
        
        public static void PreviewFarmer(SpriteBatch b, Rectangle bounds, List<RgbColour> colours, bool autoPalette)
        {
            if (GetStoredCustomizationMenu()?.GetOrCreateDisplayFarmer() is not { } farmer) return;
            
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            HsvColour lightest = colours[0].ToHsv();
            HsvColour medium = colours[1].ToHsv();
            HsvColour darkest = colours[2].ToHsv();
            
            StoredSkinTone = new SkinTone(darkest.ToXnaColor(), medium.ToXnaColor(), lightest.ToXnaColor());
            
            // StoredMedium = new HsvColour(lightest.H, lightest.S * 0.8M, lightest.V * 0.8M).ToXnaColor();
            // StoredDarkest = new HsvColour(lightest.H, lightest.S * 0.9M, lightest.V * 0.3M).ToXnaColor();
            
            farmer.FarmerRenderer.MarkSpriteDirty();
            float scale = Math.Min(bounds.Width / 72f, bounds.Height / 144f);
            drawFarmer(
                b: b,
                renderer: farmer.FarmerRenderer,
                sourceRect: farmer.FarmerSprite.SourceRect,
                position: new Vector2(bounds.Center.X - 32 * scale, bounds.Center.Y - 68 * scale),
                layerDepth: 0.8f,
                rotation: 0f,
                scale: scale,
                who: farmer
            );
            
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        }
        
        // I need my own farmer drawing functions because of a bug in the vanilla game right now where drawing them at
        // different scales leads to the shirt being misplaced... but ONLY in the downward facing direction, which is
        // the one I use... I'd rather not have to just copy these functions but oh well. TODO for 1.6.16/1.7 I guess.
        public static void drawFarmer(SpriteBatch b, FarmerRenderer renderer, Rectangle sourceRect, Vector2 position, float layerDepth, float rotation, float scale, Farmer who)
        {
            float scaledPixelZoom = 4f * scale;
            var animationFrame = new FarmerSprite.AnimationFrame(0, 100, 0, secondaryArm: false, flip: false);

            renderer.executeRecolorActions(who);
            position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));
            renderer.rotationAdjustment = Vector2.Zero;
            renderer.positionOffset.Y = animationFrame.positionOffset * 4;
            renderer.positionOffset.X = animationFrame.xOffset * 4;
            
            b.Draw(renderer.baseTexture, position + renderer.positionOffset, sourceRect, Color.White, rotation, Vector2.Zero, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Base));

            who.GetDisplayPants(out var texture, out var pantsIndex);
            Rectangle pantsRect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
            pantsRect.X += pantsIndex % 10 * 192;
            pantsRect.Y += pantsIndex / 10 * 688;
            if (!who.IsMale)
            {
                pantsRect.X += 96;
            }
            if (renderer.skin.Value != -12345 || who.pantsItem.Value != null)
            {
                b.Draw(texture, position + renderer.positionOffset, pantsRect, Utility.MakeCompletelyOpaque(who.GetPantsColor()), rotation, Vector2.Zero, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, (who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? FarmerRenderer.FarmerSpriteLayers.PantsPassedOut : FarmerRenderer.FarmerSpriteLayers.Pants));
            }
            
            sourceRect.Offset(288, 0);
            
            drawFarmerHairAndAccessories(b, renderer, who, position, scale, rotation, layerDepth);
            
            sourceRect.Offset(-288 + animationFrame.armOffset * 16, 0);
            b.Draw(renderer.baseTexture, position + renderer.positionOffset + who.armOffset, sourceRect, Color.White, rotation, Vector2.Zero, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Arms));
        }

        public static void drawFarmerHairAndAccessories(SpriteBatch b, FarmerRenderer renderer, Farmer who, Vector2 position, float scale, float rotation, float layerDepth)
        {
            int hairStyle = who.getHair();
            float scaledPixelZoom = 4f * scale;
            int frameXOffset = FarmerRenderer.featureXOffsetPerFrame[0];
            int frameYOffset = FarmerRenderer.featureYOffsetPerFrame[0];
            HairStyleMetadata hairMetadata = Farmer.GetHairStyleMetadata(hairStyle);

            renderer.executeRecolorActions(who);
            
            who.GetDisplayShirt(out var shirtTexture, out var shirtIndex);
            renderer.shirtSourceRect = new Rectangle(shirtIndex * 8 % 128, shirtIndex * 8 / 128 * 32, 8, 8);
            
            Rectangle dyedShirtSourceRect = renderer.shirtSourceRect;
            dyedShirtSourceRect.Offset(128, 0);
            
            Color hairColor = (who.prismaticHair.Value ? Utility.GetPrismaticColor() : who.hairstyleColor.Value);
            Texture2D hairTexture = hairMetadata?.texture ?? FarmerRenderer.hairStylesTexture;
            renderer.hairstyleSourceRect = ((hairMetadata != null) ? new Rectangle(hairMetadata.tileX * 16, hairMetadata.tileY * 16, 16, 32) : new Rectangle(hairStyle * 16 % FarmerRenderer.hairStylesTexture.Width, hairStyle * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32));
            
            Vector2 shirtPosition2 = position + renderer.positionOffset + new Vector2(16 * scale + frameXOffset * 4, (float)(56 + frameYOffset * 4) * scale + (float)renderer.heightOffset.Value * scale);
            b.Draw(shirtTexture, shirtPosition2, renderer.shirtSourceRect, Color.White, rotation, Vector2.Zero, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt));

            b.Draw(hairTexture, position + renderer.positionOffset + new Vector2(frameXOffset * 4, frameYOffset * 4 + ((who.IsMale && who.hair.Value >= 16) ? (-4) : ((!who.IsMale && who.hair.Value < 16) ? 4 : 0))), renderer.hairstyleSourceRect, hairColor, rotation, Vector2.Zero, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair));
            
        }
    }
}