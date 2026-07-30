using System;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using ThePhoneIsRinging.Config;

namespace ThePhoneIsRinging
{
    public class PhoneHUDMessage(Phone? phone, string message) : HUDMessage(message)
    {
        private readonly Phone phone = phone ?? throw new ArgumentNullException(nameof(phone));

        public override void draw(SpriteBatch b, int i, ref int heightUsed)
        {
            Rectangle tsarea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
            Vector2 itemBoxPosition = new Vector2(x: tsarea.Left + 16, y: tsarea.Bottom - 112 - heightUsed - 64);
            heightUsed += 112;
            
            if (Game1.isOutdoorMapSmallerThanViewport())
            {
                itemBoxPosition.X = Math.Max(val1: tsarea.Left + 16, val2: -Game1.uiViewport.X + 16);
            }
            if (Game1.uiViewport.Width < 1400)
            {
                itemBoxPosition.Y -= 48f;
            }
            
            b.Draw(
                texture: Game1.mouseCursors,
                position: itemBoxPosition,
                sourceRectangle: new Rectangle(x: 293, y: 360, width: 26, height: 24),
                color: Color.White * transparency,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 4f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );

            string messageToDraw = message;
            double secondsRemaining = ModEntry.RingTimeRemaining / 1000f;
            if (ModEntry.Config.HudTimeLeft) messageToDraw += $" {secondsRemaining,4:0.0}s";
            string toMeasure = ModEntry.Config.HudTimeLeft ? $"{message} 99.9s" : messageToDraw;
            float messageWidth = Game1.smallFont.MeasureString(text: toMeasure ?? "").X;
            
            b.Draw(
                texture: Game1.mouseCursors,
                position: new Vector2(x: itemBoxPosition.X + 104f, y: itemBoxPosition.Y),
                sourceRectangle: new Rectangle(x: 319, y: 360, width: 1, height: 24),
                color: Color.White * transparency,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: new Vector2(x: messageWidth, y: 4f),
                effects: SpriteEffects.None,
                layerDepth: 1f
            );
            
            b.Draw(
                texture: Game1.mouseCursors,
                position: new Vector2(x: itemBoxPosition.X + 104f + messageWidth, y: itemBoxPosition.Y),
                sourceRectangle: new Rectangle(x: 323, y: 360, width: 6, height: 24),
                color: Color.White * transparency,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 4f,
                effects: SpriteEffects.None,
                layerDepth: 1f
            );

            bool ringing = Phone.ringingTimer > 0 && Phone.ringingTimer < 600;
            itemBoxPosition.X += 16f + (ringing ? Game1.random.Next(-1, 2) : 0);
            itemBoxPosition.Y += 12f + (ringing ? Game1.random.Next(-1, 2) : 0);
            
            phone.drawInMenu(spriteBatch: b, location: itemBoxPosition, scaleSize: 1.2f, transparency: transparency, layerDepth: 1f, drawStackNumber: StackDrawType.Hide);

            itemBoxPosition.X += 83f;
            itemBoxPosition.Y += 24f;
            
            float lerpFactor = (float)Math.Clamp(5.15f - secondsRemaining, 0f, 0.3f) / 0.3f;
            Color textColor = Color.Lerp(Game1.textColor, Color.Red, lerpFactor - 0.2f);
            float scale = MathHelper.Lerp(1f, 1.025f, lerpFactor);
            Utility.drawTextWithShadow(b: b, text: messageToDraw ?? "", font: Game1.smallFont, position: itemBoxPosition, color: textColor * transparency, scale: scale, layerDepth: 1f, horizontalShadowOffset: -1, verticalShadowOffset: -1, shadowIntensity: transparency);
        }
        
        public override bool update(GameTime time)
        {
            if (!ModEntry.Config.HudNotification || Game1.currentLocation.NameOrUniqueName != phone.Location.NameOrUniqueName)
            {
                ModEntry.AlreadyNotified = false;
                return true;
            }

            if (string.IsNullOrWhiteSpace(Phone.whichPhoneCall))
            {
                transparency -= 0.02f;
                if (transparency < 0f)
                {
                    ModEntry.AlreadyNotified = false;
                    return true;
                }
            }
            else if (transparency < 1f) transparency = Math.Min(transparency + 0.02f, 1f);
            return false;
        }
    }
    
    internal sealed class ModEntry : Mod
    {
        internal static IMonitor ModMonitor { get; private set; } = null!;
        internal static ModConfig Config { get; private set; } = null!;
        private static Harmony Harmony { get; set; } = null!;
        
        public static double RingTimeRemaining = -1f;
        public static bool AlreadyNotified;

        public override void Entry(IModHelper helper)
        {
            i18n.Init(helper.Translation);
            ModMonitor = Monitor;
            Config = helper.ReadConfig<ModConfig>();
            Harmony = new Harmony(ModManifest.UniqueID);

            Harmony.Patch(
                original: AccessTools.Method(typeof(Phone), nameof(Phone.updateWhenCurrentLocation)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Phone_updateWhenCurrentLocation_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(Phone), nameof(Phone.Ring)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Phone_Ring_Postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(Phone), nameof(Phone.draw), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Phone_draw_Postfix))
            );
            
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Player.Warped += OnWarped;
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null) Config.SetupConfig(configMenu, ModManifest, Helper);
        }
        
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Game1.shouldTimePass()) return;
            
            RingTimeRemaining -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            RingTimeRemaining = Math.Max(0f, RingTimeRemaining);
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            AlreadyNotified = false;
        }

        public static void Phone_updateWhenCurrentLocation_Postfix(Phone __instance)
        {
            if (!Config.HudNotification) return;
            if (Game1.eventUp) return;
            if (Phone.whichPhoneCall is null)
            {
                AlreadyNotified = false;
                return;
            }
            if (AlreadyNotified) return;
            
            HUDMessage msg = new PhoneHUDMessage(__instance, i18n.PhoneRingingNotification())
            {
                messageSubject = __instance
            };
            Game1.addHUDMessage(msg);
            AlreadyNotified = true;
        }

        public static void Phone_Ring_Postfix()
        {
            if (Phone.whichPhoneCall is null)
            {
                AlreadyNotified = false;
                return;
            }
            
            float ringTime = Game1.realMilliSecondsPerGameTenMinutes * Phone.intervalsToRing;
            RingTimeRemaining = Math.Max(0f, ringTime);
        }

        public static void Phone_draw_Postfix(SpriteBatch spriteBatch, int x, int y)
        {
            if (!Config.ExclamationPoint) return;
            if (Phone.whichPhoneCall is null) return;
            
            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (y * 64 - 88) + yOffset));
            spriteBatch.Draw(Game1.mouseCursors, position, new Rectangle(403, 496, 5, 14), Color.White, 0f, new Vector2(5 / 2f, 7), 4f, SpriteEffects.None, 1f);
        }
    }
}