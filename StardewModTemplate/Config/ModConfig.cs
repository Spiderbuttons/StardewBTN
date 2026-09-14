using StardewModdingAPI;
using SpiderCore.Common.Integration;
// ReSharper disable MemberCanBePrivate.Global

namespace StardewModTemplate.Config
{
    public sealed class ModConfig
    {
        public bool Enabled { get; set; } = true;
    
        public ModConfig()
        {
            Reset();
        }

        private void Reset()
        {
            Enabled = true;
        }

        public void CreateConfigMenu(IGenericModConfigMenuApi config, IManifest ModManifest, IModHelper Helper)
        {
            config.Register(
                mod: ModManifest,
                reset: Reset,
                save: () => Helper.WriteConfig(this)
            );
        
            config.AddBoolOption(
                mod: ModManifest,
                name: () => "Enabled",
                tooltip: () => "Enable or disable this mod.",
                getValue: () => Enabled,
                setValue: value => Enabled = value
            );
        }
    }
}