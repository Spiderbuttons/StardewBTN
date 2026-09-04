using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using TheInfinityTones.Menus.ColourPickerMenu;
using TheInfinityTones.Patches;

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
        
        internal static string UNIQUE_ID => Manifest.UniqueID;
        internal static string MOD_DATA_KEY => $"{UNIQUE_ID}/SkinTone";

        private static IManifest Manifest { get; set; } = null!;
        internal static IModHelper ModHelper { get; private set; } = null!;
        internal static IMonitor ModMonitor { get; private set; } = null!;
        private static Harmony Harmony { get; set; } = null!;

        private static readonly PerScreen<Dictionary<long, SkinTone>> QueuedSkinUpdates = new(createNewState: () => new Dictionary<long, SkinTone>());

        public static readonly PerScreen<SkinTone?> StoredSkinTone = new(createNewState: () => null);
        public static readonly PerScreen<bool?> StoredPaletteToggle = new(createNewState: () => null);
        public static readonly PerScreen<bool?> StoredDarkSkinToggle = new(createNewState: () => null);
        
        private static CharacterCustomization? _storedCustomizationMenu;

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
                original: AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.overrideSnappyMenuCursorMovementBan)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(TitleMenu_overrideSnappyMenuCursorMovementBan_Postfix))
            );

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            Helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            Helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;
        }

        private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button is SButton.F2)
            {
                //
            }
        }

        private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (ModHelper.ModRegistry.IsLoaded("PeacefulEnd.FashionSense"))
            {
                FashionSensePatches.Patch(Harmony);
            }
        }

        private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (Game1.player.modData.TryGetValue(MOD_DATA_KEY, out var skinToneString))
            {
                SkinTone tone = SkinTone.FromString(skinToneString);
                BroadcastSkinChange(tone);
                Game1.player.FarmerRenderer.MarkSpriteDirty();
            }
        }

        private static void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            foreach (var (playerId, tone) in QueuedSkinUpdates.Value)
            {
                Farmer? farmer = Game1.getOnlineFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == playerId);
                if (farmer is null) continue;
                
                // We gotta wait until the modData gets synced up again but rather than wait 3-4 ticks I'm opting to just wait until we see
                // the tone that we expect to see. I figured this might work better in case the connection is laggier and takes longer to sync.
                if (!farmer.modData.TryGetValue(MOD_DATA_KEY, out var skinToneString) || skinToneString != tone.ToString())
                {
                    continue;
                }
                
                farmer.FarmerRenderer.MarkSpriteDirty();
                QueuedSkinUpdates.Value.Remove(playerId);
            }
        }

        private static void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            if (Game1.player.modData.TryGetValue(MOD_DATA_KEY, out var skinToneString))
            {
                SkinTone tone = SkinTone.FromString(skinToneString);
                BroadcastSkinChange(tone);
            }
        }

        private static void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != Manifest.UniqueID || e.Type != "SkinChange" || e.FromPlayerID == Game1.player.UniqueMultiplayerID)
                return;
            
            string toneString = e.ReadAs<string>();
            if (string.IsNullOrEmpty(toneString)) return;
            
            SkinTone tone = SkinTone.FromString(toneString);
            foreach (var farmer in Game1.getOnlineFarmers().Where(farmer => farmer.UniqueMultiplayerID == e.FromPlayerID))
            {
                QueuedSkinUpdates.Value[farmer.UniqueMultiplayerID] = tone;
            }
        }

        public static void BroadcastSkinChange(SkinTone? newTone)
        {
            ModHelper.Multiplayer.SendMessage(newTone.ToString() ?? "", "SkinChange");
        }

        private static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(asset => asset.IsEquivalentTo("Characters/Farmer/skinColors")))
            {
                SkinTone.VanillaSkinTones = null!;
            }
        }

        private static void TitleMenu_overrideSnappyMenuCursorMovementBan_Postfix(ref bool __result)
        {
            if (TitleMenu.subMenu is ColourPickerMenu picker)
            {
                __result = picker.overrideSnappyMenuCursorMovementBan();
            }
        }
        
        private static void Game1_ResetGameStateOnTitleScreen_Postfix()
        {
            StoredSkinTone.Value = null;
            StoredPaletteToggle.Value = null;
            StoredDarkSkinToggle.Value = null;
        }
        
        public static IEnumerable<Farmer> GetContextualFarmers()
        {
            foreach (var slot in (TitleMenu.subMenu as LoadGameMenu)?.MenuSlots ?? [])
            {
                if (slot is LoadGameMenu.SaveFileSlot { Farmer: not null } save) yield return save.Farmer;
            }
        
            foreach (var slot in (Game1.activeClickableMenu as FarmhandMenu)?.MenuSlots ?? [])
            {
                if (slot is LoadGameMenu.SaveFileSlot { Farmer: not null } save) yield return save.Farmer;
            }

            if (!Context.IsWorldReady) yield break;
            foreach (var farmer in Game1.getOnlineFarmers().OfType<Farmer>())
            {
                yield return farmer;
            }
        }
        
        public static Color GetStoredSkinColor(int column)
        {
            return column switch
            {
                0 => (StoredSkinTone.Value ?? SkinTone.VanillaSkinTones[0]).Darkest,
                1 => (StoredSkinTone.Value ?? SkinTone.VanillaSkinTones[0]).Medium,
                2 => (StoredSkinTone.Value ?? SkinTone.VanillaSkinTones[0]).Lightest,
                _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be 0, 1, or 2.")
            };
        }
        
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
        
        public static void PreviewFarmer(SpriteBatch b, Rectangle bounds, SkinTone skinTone, bool autoPalette)
        {
            if (GetStoredCustomizationMenu()?.GetOrCreateDisplayFarmer() is not { } farmer) return;
            
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            StoredSkinTone.Value = skinTone;
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
        private static void drawFarmer(SpriteBatch b, FarmerRenderer renderer, Rectangle sourceRect, Vector2 position, float layerDepth, float rotation, float scale, Farmer who)
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
                b.Draw(texture, position + renderer.positionOffset, pantsRect, Utility.MakeCompletelyOpaque(who.GetPantsColor()), rotation, Vector2.Zero, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, who.FarmerSprite.CurrentAnimationFrame.frame == 5 ? FarmerRenderer.FarmerSpriteLayers.PantsPassedOut : FarmerRenderer.FarmerSpriteLayers.Pants));
            }
            
            sourceRect.Offset(288, 0);
            
            drawFarmerHairAndAccessories(b, renderer, who, position, scale, rotation, layerDepth);
            
            sourceRect.Offset(-288 + animationFrame.armOffset * 16, 0);
            b.Draw(renderer.baseTexture, position + renderer.positionOffset + who.armOffset, sourceRect, Color.White, rotation, Vector2.Zero, scaledPixelZoom, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Arms));
        }

        private static void drawFarmerHairAndAccessories(SpriteBatch b, FarmerRenderer renderer, Farmer who, Vector2 position, float scale, float rotation, float layerDepth)
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
            
            Color hairColor = who.prismaticHair.Value ? Utility.GetPrismaticColor() : who.hairstyleColor.Value;
            Texture2D hairTexture = hairMetadata?.texture ?? FarmerRenderer.hairStylesTexture;
            renderer.hairstyleSourceRect = hairMetadata != null ? new Rectangle(hairMetadata.tileX * 16, hairMetadata.tileY * 16, 16, 32) : new Rectangle(hairStyle * 16 % FarmerRenderer.hairStylesTexture.Width, hairStyle * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
            
            Vector2 shirtPosition2 = position + renderer.positionOffset + new Vector2(16 * scale + frameXOffset * 4, (56 + frameYOffset * 4) * scale + renderer.heightOffset.Value * scale);
            b.Draw(shirtTexture, shirtPosition2, renderer.shirtSourceRect, Color.White, rotation, Vector2.Zero, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Shirt));

            b.Draw(hairTexture, position + renderer.positionOffset + new Vector2(frameXOffset * 4, frameYOffset * 4 + (who.IsMale && who.hair.Value >= 16 ? -4 : !who.IsMale && who.hair.Value < 16 ? 4 : 0)), renderer.hairstyleSourceRect, hairColor, rotation, Vector2.Zero, scaledPixelZoom, SpriteEffects.None, FarmerRenderer.GetLayerDepth(layerDepth, FarmerRenderer.FarmerSpriteLayers.Hair));
            
        }
    }
}