using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace NoRedecoratingPlease;

internal sealed class ModEntry : Mod
{
    private static IMonitor ModMonitor { get; set; } = null!;
    private static Harmony Harmony { get; set; } = null!;

    public override void Entry(IModHelper helper)
    {
        ModMonitor = Monitor;
        Harmony = new Harmony(ModManifest.UniqueID);
            
        Harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
            transpiler: new HarmonyMethod(typeof(ModEntry), nameof(NPC_marriageDuties_Transpiler))
        );
    }

    public static IEnumerable<CodeInstruction> NPC_marriageDuties_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase original)
    {
        var code = instructions.ToList();
        try
        {
            var matcher = new CodeMatcher(code, il);

            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldc_R8, 0.045)).ThrowIfNotMatch($"Failed to find entry point.");
            matcher.Operand = -1.0;

            return matcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            ModMonitor.Log($"Failed to transpile {original.DeclaringType?.FullName}::{original.Name}(): {e}", LogLevel.Error);
            return code;
        }
    }
}