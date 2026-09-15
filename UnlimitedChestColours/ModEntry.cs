using HarmonyLib;
using SpiderCore.Common.Logging;
using SpiderCore.Common.Shaders;
using StardewModdingAPI;
using StardewModdingAPI.Events;

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
        }
    }
}