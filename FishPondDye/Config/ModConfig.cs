using System;
using FishPondDye.APIs;
using StardewModdingAPI;
// ReSharper disable MemberCanBePrivate.Global

namespace FishPondDye.Config;

public sealed class ModConfig
{
    public int DyePrice { get; set; } = 50;
    public PrismaticSynchronizationMode PrismaticSynchronizationMode { get; set; } = PrismaticSynchronizationMode.Random;
    public float PrismaticSpeedMultiplier = 1f;

    public ModConfig()
    {
        Init();
    }

    private void Init()
    {
        DyePrice = 50;
        PrismaticSynchronizationMode = PrismaticSynchronizationMode.Random;
        PrismaticSpeedMultiplier = 1f;
    }

    public void SetupConfig(IGenericModConfigMenuApi configMenu, IManifest ModManifest, IModHelper Helper)
    {
        configMenu.Register(
            mod: ModManifest,
            reset: Init,
            save: () => Helper.WriteConfig(this)
        );

        configMenu.AddNumberOption(
            mod: ModManifest,
            name: i18n.Config_DyePrice_Name,
            tooltip: i18n.Config_DyePrice_Description,
            min: 0,
            getValue: () => DyePrice,
            setValue: value => DyePrice = value
        );
        
        // Add the sync one as a text option so we get the nice drop-down.
        // Casey add AddEnumOption to GMCM 2.0 pls
        configMenu.AddTextOption(
            mod: ModManifest,
            name: i18n.Config_PrismaticSync_Name,
            tooltip: i18n.Config_PrismaticSync_Description,
            getValue: () => PrismaticSynchronizationMode.ToString(),
            setValue: value =>
            {
                if (Enum.TryParse<PrismaticSynchronizationMode>(value, out var mode))
                {
                    PrismaticSynchronizationMode = mode;
                }
            },
            allowedValues:
            [
                nameof(PrismaticSynchronizationMode.Synchronized),
                nameof(PrismaticSynchronizationMode.Random),
                nameof(PrismaticSynchronizationMode.HorizontalPosition),
                nameof(PrismaticSynchronizationMode.VerticalPosition),
                nameof(PrismaticSynchronizationMode.HorizontalAndVerticalPosition)
            ],
            formatAllowedValue: value => value switch
            {
                nameof(PrismaticSynchronizationMode.Synchronized) => i18n.Config_PrismaticSync_Synchronized(),
                nameof(PrismaticSynchronizationMode.Random) => i18n.Config_PrismaticSync_Random(),
                nameof(PrismaticSynchronizationMode.HorizontalPosition) => i18n.Config_PrismaticSync_HorizontalPosition(),
                nameof(PrismaticSynchronizationMode.VerticalPosition) => i18n.Config_PrismaticSync_VerticalPosition(),
                nameof(PrismaticSynchronizationMode.HorizontalAndVerticalPosition) => i18n.Config_PrismaticSync_HorizontalAndVerticalPosition(),
                _ => value
            }
        );
        
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: i18n.Config_PrismaticSpeed_Name,
            tooltip: i18n.Config_PrismaticSpeed_Description,
            min: 0f,
            max: 2f,
            getValue: () => PrismaticSpeedMultiplier,
            setValue: value => PrismaticSpeedMultiplier = value,
            formatValue: value => $"{value:0.00}x"
        );
    }
}