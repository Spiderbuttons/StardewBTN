using GenericModConfigMenu;
using StardewModdingAPI;

namespace ThePhoneIsRinging.Config;

public sealed class ModConfig
{
    public bool HudNotification { get; set; } = true;
    public bool HudTimeLeft { get; set; } = true;
    public bool ExclamationPoint { get; set; } = true;

    public ModConfig()
    {
        Init();
    }

    private void Init()
    {
        HudNotification = true;
        ExclamationPoint = true;
    }

    public void SetupConfig(IGenericModConfigMenuApi configMenu, IManifest ModManifest, IModHelper Helper)
    {
        configMenu.Register(
            mod: ModManifest,
            reset: Init,
            save: () => Helper.WriteConfig(this)
        );

        configMenu.AddBoolOption(
            mod: ModManifest,
            name: i18n.Config_HudNotification_Name,
            tooltip: i18n.Config_HudNotification_Description,
            getValue: () => HudNotification,
            setValue: value => HudNotification = value
        );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: i18n.Config_HudTimeLeft_Name,
            tooltip: i18n.Config_HudTimeLeft_Description,
            getValue: () => HudTimeLeft,
            setValue: value => HudTimeLeft = value
        );
        
        configMenu.AddBoolOption(
            mod: ModManifest,
            name: i18n.Config_ExclamationPoint_Name,
            tooltip: i18n.Config_ExclamationPoint_Description,
            getValue: () => ExclamationPoint,
            setValue: value => ExclamationPoint = value
        );
    }
}