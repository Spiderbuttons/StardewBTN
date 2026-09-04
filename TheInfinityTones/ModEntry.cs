using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using TheInfinityTones.Helpers;
using TheInfinityTones.Menus.ColourPickerMenu;

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

        public static readonly ConditionalWeakTable<Farmer, FarmerRenderer> FarmerToRendererMap = [];
        public static readonly PerScreen<Dictionary<long, SkinTone>> QueuedSkinUpdates = new(createNewState: () => new Dictionary<long, SkinTone>());

        public static readonly PerScreen<SkinTone?> StoredSkinTone = new(createNewState: () => null);
        public static readonly PerScreen<bool?> StoredPaletteToggle = new(createNewState: () => null);
        public static readonly PerScreen<bool?> StoredDarkSkinToggle = new(createNewState: () => null);

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
                original: AccessTools.Method(typeof(FarmerRenderer), nameof(FarmerRenderer.ApplySkinColor)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(FarmerRenderer_ApplySkinColor_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.createdNewCharacter)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(TitleMenu_createdNewCharacter_Prefix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(LoadGameMenu), nameof(LoadGameMenu.addSaveFiles)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(LoadGameMenu_addSaveFiles_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.overrideSnappyMenuCursorMovementBan)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(TitleMenu_overrideSnappyMenuCursorMovementBan_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.farmerInit)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Farmer_farmerInit_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(SaveGame), nameof(SaveGame.loadDataToFarmer)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(SaveGame_loadDataToFarmer_Postfix))
            );

            Helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            
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
            if (e.Button is SButton.F2 or SButton.LeftStick && Context.IsMainPlayer)
            {
                foreach (var (farmer, renderer) in FarmerToRendererMap)
                {
                    Log.Alert($"FarmerRenderer: {renderer}, Farmer: {farmer.Name}");
                    renderer.MarkSpriteDirty();
                }
            }
            
            if (!Context.IsWorldReady)
                return;
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Log.Alert("Save loaded");
            FarmerToRendererMap.Clear();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                FarmerToRendererMap.AddOrUpdate(farmer, farmer.FarmerRenderer);
                farmer.FarmerRenderer.MarkSpriteDirty();
            }
        }

        public static void BroadcastSkinChange(SkinTone? newTone)
        {
            ModHelper.Multiplayer.SendMessage(newTone.ToString() ?? "", "SkinChange");
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != Manifest.UniqueID || e.Type != "SkinChange" || e.FromPlayerID == Game1.player.UniqueMultiplayerID)
                return;

            Log.Alert("Received skin change message");
            SkinTone tone = SkinTone.FromString(e.ReadAs<string>());
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (farmer.UniqueMultiplayerID != e.FromPlayerID) continue;
                FarmerToRendererMap.AddOrUpdate(farmer, farmer.FarmerRenderer);
                QueuedSkinUpdates.Value[farmer.UniqueMultiplayerID] = tone;
            }
        }

        private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            return;
            foreach (var (playerId, tone) in QueuedSkinUpdates.Value)
            {
                Farmer? farmer = Game1.getOnlineFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == playerId);
                if (farmer is null) continue;
                
                // We gotta wait until the modData gets synced up again but rather than wait 3-4 ticks I'm opting to just wait until we see
                // the tone that we expect to see. I figured this might work better in case of laggier connections.
                if (!farmer.modData.TryGetValue($"{Manifest.UniqueID}/SkinTone", out var skinToneString) || skinToneString != tone.ToString())
                {
                    Log.Info($"Farmer {farmer.Name} has not yet synced skin tone. Expected: {tone}, Actual: {skinToneString}. Waiting...");
                    continue;
                }
                
                Log.Alert($"Farmer {farmer.Name} has synced skin tone. Expected: {tone}, Actual: {skinToneString}. Updating renderer...");
                FarmerToRendererMap.AddOrUpdate(farmer, farmer.FarmerRenderer);
                farmer.FarmerRenderer.MarkSpriteDirty();
                QueuedSkinUpdates.Value.Remove(playerId);
            }
        }
        
        private static void Farmer_farmerInit_Postfix(Farmer __instance)
        {
            // Log.Warn($"Farmer {__instance.Name} initialized. Adding to FarmerToRendererMap.");
            // // find the farmer with the same uniqueID and update that entry
            // foreach (var (farmer, renderer) in FarmerToRendererMap)
            // {
            //     if (farmer.UniqueMultiplayerID == __instance.UniqueMultiplayerID)
            //     {
            //         Log.Warn($"Found existing farmer {farmer.Name} with same UniqueMultiplayerID. Updating entry.");
            //         FarmerToRendererMap.Remove(farmer);
            //         break;
            //     }
            // }
            // FarmerToRendererMap.AddOrUpdate(__instance, __instance.FarmerRenderer);
        }

        private static void SaveGame_loadDataToFarmer_Postfix(Farmer target)
        {
            Log.Info($"Loaded data to farmer {target.Name}. Updating FarmerToRendererMap.");
            foreach (var (farmer, renderer) in FarmerToRendererMap)
            {
                if (farmer.UniqueMultiplayerID == target.UniqueMultiplayerID)
                {
                    Log.Warn($"Found existing farmer {farmer.Name} with same UniqueMultiplayerID. Updating entry.");
                    FarmerToRendererMap.Remove(farmer);
                    break;
                }
            }
            FarmerToRendererMap.AddOrUpdate(target, target.FarmerRenderer);
        }

        private static void TitleMenu_createdNewCharacter_Prefix()
        {
            // if (StoredSkinTone.Value is null || Game1.player is null) return;
            //
            // Game1.player.modData[$"{Manifest.UniqueID}/SkinTone"] = StoredSkinTone.Value.ToString();
            // Game1.player.modData[$"{Manifest.UniqueID}/DarkSkin"] = StoredPaletteToggle.Value?.ToString() ?? "false";
            //
            // StoredSkinTone.Value = null;
            // StoredPaletteToggle.Value = null;
            // StoredDarkSkinToggle.Value = null;
        }
        
        private static void TitleMenu_overrideSnappyMenuCursorMovementBan_Postfix(TitleMenu __instance, ref bool __result)
        {
            if (TitleMenu.subMenu is ColourPickerMenu picker)
            {
                __result = picker.overrideSnappyMenuCursorMovementBan();
            }
        }

        private static void LoadGameMenu_addSaveFiles_Postfix(LoadGameMenu __instance, List<Farmer> files)
        {
            FarmerToRendererMap.Clear();
            foreach (var farmer in files)
            {
                FarmerToRendererMap.AddOrUpdate(farmer, farmer.FarmerRenderer);
            }
        }
        
        private static void Game1_ResetGameStateOnTitleScreen_Postfix()
        {
            StoredSkinTone.Value = null;
            StoredPaletteToggle.Value = null;
            StoredDarkSkinToggle.Value = null;
        }

        private static void FarmerRenderer_ApplySkinColor_Postfix(FarmerRenderer __instance, string texture_name,
            Color[] pixels)
        {
            if (StoredSkinTone.Value is not null)
            {
                __instance._SwapColor(texture_name, pixels, 260, GetSkinColor(0));
                __instance._SwapColor(texture_name, pixels, 261, GetSkinColor(1));
                __instance._SwapColor(texture_name, pixels, 262, GetSkinColor(2));
                return;
            }
            
            Farmer? farmer = GetFarmersToCheck().FirstOrDefault(f => f.FarmerRenderer == __instance);
            if (farmer is null) return;
            
            SkinTone skinTone = GetSkinToneFromFarmer(farmer);
            __instance._SwapColor(texture_name, pixels, 260, skinTone.Darkest);
            __instance._SwapColor(texture_name, pixels, 261, skinTone.Medium);
            __instance._SwapColor(texture_name, pixels, 262, skinTone.Lightest);
        }

        private static IEnumerable<Farmer> GetFarmersToCheck()
        {
            if (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is LoadGameMenu menu)
            {
                foreach (var slot in menu.MenuSlots)
                {
                    if (slot is LoadGameMenu.SaveFileSlot { Farmer: not null } save) yield return save.Farmer;
                }
            }
            
            if (Game1.activeClickableMenu is FarmhandMenu farmhandMenu)
            {
                foreach (var slot in farmhandMenu.MenuSlots)
                {
                    if (slot is LoadGameMenu.SaveFileSlot { Farmer: not null } save) yield return save.Farmer;
                }
            }

            if (Context.IsWorldReady)
            {
                foreach (var farmer in Game1.getOnlineFarmers())
                {
                    yield return farmer;
                }
            }
        }

        private static SkinTone GetSkinToneFromFarmer(Farmer who)
        {
            if (!who.modData.TryGetValue($"{Manifest.UniqueID}/SkinTone", out var skinToneString))
            {
                Log.Debug("Farmer has no skin tone mod data, using vanilla skin tone.");
                return SkinTone.VanillaSkinTones.ElementAtOrDefault(who.skin.Value);
            }
            try
            {
                Log.Debug($"Farmer has skin tone mod data: {skinToneString}, parsing...");
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
                0 => StoredSkinTone.Value?.Darkest ?? SkinTone.VanillaSkinTones[0].Darkest,
                1 => StoredSkinTone.Value?.Medium ?? SkinTone.VanillaSkinTones[0].Medium,
                2 => StoredSkinTone.Value?.Lightest ?? SkinTone.VanillaSkinTones[0].Lightest,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be 0, 1, or 2.")
            };
        }
        
        private static CharacterCustomization? _storedCustomizationMenu;
        
        public static void RestoreCustomizationMenu()
        {
            if (_storedCustomizationMenu is not null)
            {
                if (Game1.activeClickableMenu is TitleMenu) TitleMenu.subMenu = _storedCustomizationMenu;
                else Game1.activeClickableMenu = _storedCustomizationMenu;
                
                _storedCustomizationMenu.RemoveDependency();
                _storedCustomizationMenu.ResetComponents();
                _storedCustomizationMenu.populateClickableComponentList();
                _storedCustomizationMenu = null;
            }
        }

        public static void StoreCustomizationMenu()
        {
            _storedCustomizationMenu = Game1.activeClickableMenu is TitleMenu ? TitleMenu.subMenu as CharacterCustomization : Game1.activeClickableMenu as CharacterCustomization;
            _storedCustomizationMenu?.AddDependency();
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
            
            StoredSkinTone.Value = new SkinTone(darkest.ToXnaColor(), medium.ToXnaColor(), lightest.ToXnaColor());
            
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