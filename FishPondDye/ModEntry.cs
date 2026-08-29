using System;
using System.Linq;
using FishPondDye.APIs;
using HarmonyLib;
using Microsoft.Xna.Framework;
using FishPondDye.Config;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using FishPondDye.Helpers;
using FishPondDye.Menus.ColourPickerMenu;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using Object = StardewValley.Object;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FishPondDye
{
    public enum PrismaticSynchronizationMode
    {
        Synchronized = 0,
        Random = 1,
        HorizontalPosition = 2,
        VerticalPosition = 3,
        HorizontalAndVerticalPosition = 4,
    }
    
    [HarmonyPatch(typeof(FishPond))]
    internal sealed class ModEntry : Mod
    {
        internal static string UNIQUE_ID = null!;
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        internal static ModConfig Config { get; set; } = null!;
        internal static Harmony Harmony { get; set; } = null!;

        // This one is only used for drawing the preview in the colour picker menu.
        private static FishPond? DummyPond;

        public override void Entry(IModHelper helper)
        {
            UNIQUE_ID = ModManifest.UniqueID;
            i18n.Init(helper.Translation);
            ModHelper = helper;
            ModMonitor = Monitor;
            Config = helper.ReadConfig<ModConfig>();
            Harmony = new Harmony(UNIQUE_ID);

            Harmony.PatchAll();

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null) Config.SetupConfig(configMenu, ModManifest, Helper);
        }
        
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"{UNIQUE_ID}/Objects"))
            {
                e.LoadFromModFile<Texture2D>("assets/bottles.png", AssetLoadPriority.Medium);
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    string[] colours = ["Prismatic", "Red", "Orange", "Green", "Blue", "Purple", "Custom"];
                    var data = asset.AsDictionary<string, ObjectData>().Data;
                    for (var i = 0; i < colours.Length; i++)
                    {
                        var colour = colours[i];
                        data[$"{UNIQUE_ID}_DyeBottle_{colour}"] = new ObjectData
                        {
                            Name = $"{UNIQUE_ID}_DyeBottle_{colour}",
                            DisplayName = $"{i18n.DyeBottleName()} ({i18n.GetByKey(colour)})",
                            Description = i18n.DyeBottleDescription(),
                            Type = "Basic",
                            Category = Object.sellAtFishShopCategory,
                            Price = 50,
                            Texture = $"{UNIQUE_ID}/Objects",
                            SpriteIndex = i,
                            Edibility = -50,
                            IsDrink = true,
                            Buffs = [
                                new ObjectBuffData
                                {
                                    Id = $"{UNIQUE_ID}_DyeBottle_Debuff",
                                    BuffId = "25"
                                }
                            ],
                            CanBeGivenAsGift = false,
                            ExcludeFromFishingCollection = true,
                            ExcludeFromShippingCollection = true,
                            ExcludeFromRandomSale = true,
                            ContextTags = [
                                $"color_{(colour != "Custom" ? colour.ToLowerInvariant() : "white")}",
                                $"{UNIQUE_ID.ToLowerInvariant()}_dye_source"
                            ]
                        };
                    }
                    data[$"{UNIQUE_ID}_DyeRemover"] = new ObjectData
                    {
                        Name = $"{UNIQUE_ID}_DyeRemover",
                        DisplayName = i18n.DyeRemoverName(),
                        Description = i18n.DyeRemoverDescription(),
                        Type = "Basic",
                        Category = Object.sellAtFishShopCategory,
                        Price = 50,
                        Texture = $"{UNIQUE_ID}/Objects",
                        SpriteIndex = 7,
                        CanBeGivenAsGift = false,
                        ExcludeFromFishingCollection = true,
                        ExcludeFromShippingCollection = true,
                        ExcludeFromRandomSale = true,
                        ContextTags = [
                            $"{UNIQUE_ID.ToLowerInvariant()}_dye_remover"
                        ]
                    };
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                {
                    string[] colours = ["Prismatic", "Red", "Orange", "Green", "Blue", "Purple", "Custom"];
                    var data = asset.AsDictionary<string, ShopData>().Data["FishShop"].Items;
                    foreach (var colour in colours)
                    {
                        ShopItemData item = new ShopItemData
                        {
                            Id = $"{UNIQUE_ID}_DyeBottle_{colour}",
                            ItemId = $"(O){UNIQUE_ID}_DyeBottle_{colour}",
                            Price = 50,
                            Condition = """
                                        BUILDINGS_CONSTRUCTED All "Fish Pond"
                                        """
                        };
                        data.Add(item);
                    }
                    ShopItemData dyeRemover = new ShopItemData
                    {
                        Id = $"{UNIQUE_ID}_DyeRemover",
                        ItemId = $"(O){UNIQUE_ID}_DyeRemover",
                        Price = 50,
                        Condition = """
                                    BUILDINGS_CONSTRUCTED All "Fish Pond"
                                    """
                    };
                    data.Add(dyeRemover);
                });
            }
        }
        
        [HarmonyPostfix, HarmonyPatch(nameof(FishPond.performActiveObjectDropInAction))]
        public static void FishPond_performActiveObjectDropInAction_Postfix(FishPond __instance, Farmer who, bool probe, ref bool __result)
        {
            if (__result) return;
            if (who.ActiveObject is not { } heldObject) return;
            if (!heldObject.QualifiedItemId.StartsWith($"(O){UNIQUE_ID}")) return;

            if (probe)
            {
                __result = true;
                return;
            }
        
            who.reduceActiveItemByOne();

            Color targetColour;
            string[] idSplit = heldObject.QualifiedItemId.Split('_');
            if (idSplit.Length < 3) targetColour = new Color(60, 126, 150);
            else
            {
                string bottleColour = idSplit[2];
                if (bottleColour is "Custom")
                {
                    Game1.playSound("bigSelect");
                    Game1.activeClickableMenu = new ColourPickerMenu(drawPreview: DrawPondPreview, onConfirm: (rgb) =>
                    {
                        if (rgb == new RgbColour(255, 255, 255))
                        {
                            // The game has special handling for exactly white to change it to the current location's water colour. So we fudge it a little bit here so the player is still able to actually see white if they want white. They won't notice this very subtle difference.
                            rgb = new RgbColour(254, 254, 254);
                        }
                        Color colour = rgb.ToXnaColor();
                        __instance.showObjectThrownIntoPondAnimation(who, heldObject, () => dropDyeIntoPond(__instance, heldObject, colour));
                    });
                    __result = true;
                    return;
                }
            
                targetColour = bottleColour switch 
                {
                    "Red" => Color.Red,
                    "Orange" => new Color(255, 128, 0),
                    "Green" => Color.Green,
                    "Blue" => new Color(112, 112, 255),
                    "Purple" => new Color(255, 69, 255),
                    "Prismatic" => Utility.GetPrismaticColor(),
                    _ => new Color(60, 126, 150)
                };
            }
        
            __instance.showObjectThrownIntoPondAnimation(who, heldObject, () => dropDyeIntoPond(__instance, heldObject, targetColour));
            __result = true;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(FishPond.Update))]
        public static void FishPond_Update_Postfix(FishPond __instance, GameTime time)
        {
            if (GetDoubleFromModData(__instance, $"{UNIQUE_ID}/DyeTime") is { } existingDyeTime && existingDyeTime > Game1.currentGameTime.TotalGameTime.TotalMilliseconds)
            {
                // If the existing DyeTime is more than the currentGameTime, the save must've just been loaded.
                // Setting it to -3000 makes it so that the pond immediately becomes the colour it's supposed to be.
                // Otherwise, the pond would be stuck at the default colour until the player played for the same amount of time
                // that they did last time before dying the pond.
                __instance.modData[$"{UNIQUE_ID}/DyeTime"] = $"-3000";
            }
            
            if (__instance.modData.ContainsKey($"{UNIQUE_ID}/Prismatic"))
            {
                PrismaticSynchronizationMode syncMode = Config.PrismaticSynchronizationMode;
                int idOffset = syncMode switch 
                {
                    PrismaticSynchronizationMode.Random => __instance.id.Value.ToByteArray().Sum(b => b),
                    PrismaticSynchronizationMode.Synchronized => 0,
                    PrismaticSynchronizationMode.HorizontalPosition => __instance.tileX.Value / __instance.tilesWide.Value,
                    PrismaticSynchronizationMode.VerticalPosition => __instance.tileY.Value / __instance.tilesHigh.Value,
                    PrismaticSynchronizationMode.HorizontalAndVerticalPosition => (__instance.tileX.Value / __instance.tilesWide.Value) + (__instance.tileY.Value / __instance.tilesHigh.Value),
                    _ => 0
                };
                __instance.modData[$"{UNIQUE_ID}/TargetColour"] = $"{Utility.GetPrismaticColor(offset: idOffset, speedMultiplier: Config.PrismaticSpeedMultiplier).PackedValue}";
            }
            
            if (GetColourFromModData(__instance, $"{UNIQUE_ID}/TargetColour") is { } targetColour)
            {
                Color startingColour = GetColourFromModData(__instance, $"{UNIQUE_ID}/StartingColour") ?? (__instance.GetWaterColor(Vector2.Zero) ?? __instance.GetParentLocation().waterColor.Value);

                double dyeTime = GetDoubleFromModData(__instance, $"{UNIQUE_ID}/DyeTime") ?? Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            
                double timeSinceDye = Game1.currentGameTime.TotalGameTime.TotalMilliseconds - dyeTime - 1000;
                double transitionDuration = 2000;
                float transitionProgress = (float)Math.Min(timeSinceDye / transitionDuration, 1.0);
            
                Color newColour = Color.Lerp(startingColour, targetColour, transitionProgress);

                __instance.overrideWaterColor.Value = newColour;
                if (newColour == targetColour && targetColour == new Color(60, 126, 150))
                {
                    __instance.overrideWaterColor.Value = Color.White;
                    __instance.modData.Remove($"{UNIQUE_ID}/TargetColour");
                    __instance.modData.Remove($"{UNIQUE_ID}/CurrentColour");
                    __instance.modData.Remove($"{UNIQUE_ID}/StartingColour");
                    __instance.modData.Remove($"{UNIQUE_ID}/DyeTime");
                    __instance.modData.Remove($"{UNIQUE_ID}/BubbleTimer");
                    __instance.modData.Remove($"{UNIQUE_ID}/Prismatic");
                    Game1.playSound("steam");
                }
                else
                {
                    if (newColour == targetColour && __instance.modData.Remove($"{UNIQUE_ID}/BubbleTimer")) Game1.playSound("steam");
                    __instance.modData[$"{UNIQUE_ID}/CurrentColour"] = $"{newColour.PackedValue}";
                }
            }

            if (GetColourFromModData(__instance, $"{UNIQUE_ID}/CurrentColour") is not { } currentColour) return;
            
            __instance.overrideWaterColor.Value = currentColour;
            
            if (GetDoubleFromModData(__instance, $"{UNIQUE_ID}/BubbleTimer") is not { } bubbleTimer) return;
                
            bubbleTimer -= time.ElapsedGameTime.TotalMilliseconds;
            if (bubbleTimer <= 0)
            {
                spawnBubblesOnPond(__instance, currentColour);
                __instance.modData[$"{UNIQUE_ID}/BubbleTimer"] = "100";
            }
            else
            {
                __instance.modData[$"{UNIQUE_ID}/BubbleTimer"] = $"{bubbleTimer}";
            }
        }
    
        private static void dropDyeIntoPond(FishPond pond, Object dye, Color color)
        {
            pond.modData[$"{UNIQUE_ID}/BubbleTimer"] = $"100";
            pond.modData[$"{UNIQUE_ID}/DyeTime"] = $"{Game1.currentGameTime.TotalGameTime.TotalMilliseconds}";
            pond.modData[$"{UNIQUE_ID}/StartingColour"] = $"{(pond.GetWaterColor(Vector2.Zero) ?? new Color(60, 126, 150)).PackedValue}";
            pond.modData[$"{UNIQUE_ID}/TargetColour"] = $"{color.PackedValue}";
            if (dye.QualifiedItemId.ContainsIgnoreCase("Prismatic")) pond.modData[$"{UNIQUE_ID}/Prismatic"] = "true";
            else pond.modData.Remove($"{UNIQUE_ID}/Prismatic");
            Game1.playSound("slosh");
            Game1.playSound("bubbles");
        }

        private static void spawnBubblesOnPond(FishPond pond, Color color)
        {
            for (int i = 0; i < 5; i++)
            {
                Rectangle pondBounds = pond.GetBoundingBox();
                int pondWidth = pond.tilesWide.Value;
                int pondHeight = pond.tilesHigh.Value;
                int bubbleX = Game1.random.Next(pondBounds.X + Game1.tileSize - 15, pondBounds.X + (pondWidth - 1) * 64 - 15);
                int bubbleY = Game1.random.Next(pondBounds.Y + Game1.tileSize, pondBounds.Y + (pondHeight - 1) * 64);
                float xJitter = Game1.random.Next(-10, 11) / 10f / 1.5f;
                Vector2 bubblePosition = new Vector2(bubbleX, bubbleY);
                var tas = new TemporaryAnimatedSprite(
                    textureName: "LooseSprites\\Cursors",
                    sourceRect: new Rectangle(372, 1956, 10, 10),
                    position: bubblePosition,
                    flipped: false,
                    alphaFade: 0.002f,
                    color: color
                ) {
                    alpha = 0.75f,
                    motion = new Vector2(x: xJitter, y: -1.5f),
                    acceleration = new Vector2(x: -0.002f, y: 0f),
                    interval = 99999f,
                    layerDepth = 1f,
                    scale = 3f,
                    scaleChange = 0.01f,
                    rotationChange = Game1.random.Next(minValue: -5, maxValue: 6) * (float)Math.PI / 256f
                };
                pond.GetParentLocation().temporarySprites.Add(tas);
            }
        }

        private static void DrawPondPreview(SpriteBatch b, Rectangle bounds, RgbColour colour)
        {
            DummyPond ??= new FishPond();
            if (DummyPond.texture?.Value is not { } texture)
                return;
            
            Color xnaColour = colour.ToXnaColor();
            const float alpha = 0.5f;
            if (xnaColour == Color.White)
            {
                // The game has special handling for exactly white to change it to the current location's water colour.
                // So we fudge it a little bit here so the player is still able to actually see white if they want white.
                xnaColour = new Color(254, 254, 254);
            }
            DummyPond.overrideWaterColor.Value = xnaColour;
            
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
                        color: DummyPond.overrideWaterColor.Value * alpha,
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
        
        private static Color? GetColourFromModData(IHaveModData? source, string key)
        {
            if (source?.modData.TryGetValue(key, out var packedColourString) == true &&
                uint.TryParse(packedColourString, out uint packedColour))
            {
                return new Color(packedColour);
            }
            return null;
        }
    
        private static double? GetDoubleFromModData(IHaveModData? source, string key)
        {
            if (source?.modData.TryGetValue(key, out var valueString) == true &&
                double.TryParse(valueString, out double value))
            {
                return value;
            }
            return null;
        }
        
        private static int? GetIntFromModData(IHaveModData? source, string key)
        {
            if (source?.modData.TryGetValue(key, out var valueString) == true &&
                int.TryParse(valueString, out int value))
            {
                return value;
            }
            return null;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button is SButton.F3)
            {
                if (Game1.activeClickableMenu is not null) Game1.activeClickableMenu.exitThisMenu();
                else
                {
                    Game1.activeClickableMenu = new ColourPickerMenu(drawPreview: DrawPondPreview, onConfirm: (colour) => 
                    {
                        Log.Warn("Confirmed: " + colour);
                    }, onCancel: (colour) => 
                    {
                        Log.Warn("Cancelled: " + colour);
                    });
                }
            }
        }
    }
}