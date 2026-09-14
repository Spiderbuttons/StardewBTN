using HarmonyLib;
#if (EnableConfig)
using SpiderCore.Common.Integration;
#endif
#if (EnableConsoleCommands)
using SpiderCore.Common.Commands;
#endif
using SpiderCore.Common.Logging;
#if (EnableShaders)
using SpiderCore.Common.Shaders;
#endif
using StardewModdingAPI;
using StardewModdingAPI.Events;
#if (EnableConfig)
using StardewModTemplate.Config;
#endif
// ReSharper disable MemberCanBePrivate.Global

namespace StardewModTemplate
{
    internal sealed class ModEntry : Mod
    {
        internal static string UNIQUE_ID => Manifest.UniqueID;

        internal static IManifest Manifest { get; set; } = null!;
        internal static IModHelper ModHelper { get; set; } = null!;
        #if (EnableConfig)
        internal static ModConfig Config { get; set; } = null!;
        #endif
        #if (EnableConsoleCommands)
        internal static CommandHandler CommandHandler { get; set; } = null!;
        #endif
        internal static Harmony Harmony { get; set; } = null!;

        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;
            Manifest = ModManifest;
            ModHelper = helper;
            #if (EnableTranslations)
            i18n.Init(ModHelper.Translation);
            #endif
            #if (EnableShaders)
            ShaderUtilities.Helper = ModHelper;
            #endif
            #if (EnableConsoleCommands)
            CommandHandler = new CommandHandler(ModHelper, Manifest, "$(RootCommand)");
            CommandHandler.Register();
            #endif

            Harmony = new Harmony(ModManifest.UniqueID);
            Harmony.PatchAll();

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            #if (EnableConfig)
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            #endif
        }

        #if (EnableConfig)
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null) Config.CreateConfigMenu(configMenu, ModManifest, Helper);
        }
        
        #endif
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
        }
    }
}