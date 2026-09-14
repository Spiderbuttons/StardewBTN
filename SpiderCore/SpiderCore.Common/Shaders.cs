using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using SpiderCore.Common.Logging;
using StardewModdingAPI;
using StardewValley;

namespace SpiderCore.Common.Shaders
{
    public static class ShaderUtilities
    {
        public static IModHelper Helper { get; set; } = null!;
        
        private static DirectoryInfo? DevelopmentDirectory;

        private static DirectoryInfo? ShaderDirectory =>
            DevelopmentDirectory?.GetDirectories(Path.Combine("assets", "shaders")).FirstOrDefault();

        private static DirectoryInfo? FxDirectory => DevelopmentDirectory?.GetDirectories("fx").FirstOrDefault();

        private static readonly Dictionary<string, FileSystemWatcher> ShaderWatchers = new();
        private static readonly Dictionary<string, FileSystemWatcher> SourceWatchers = new();

        public static Effect LoadShader(string shaderName)
        {
            if (Helper is null) throw new NullReferenceException(nameof(Helper));
            byte[] stream = File.ReadAllBytes(Path.Combine(Helper.DirectoryPath, "assets", "shaders", $"{shaderName}.mgfx"));
            return new Effect(Game1.graphics.GraphicsDevice, stream);
        }

        public static void WatchShader(string shaderName, Action<Effect> onChange, [CallerFilePath] string? callerFilePath = null)
        {
            DevelopmentDirectory ??= FindSourceDirectory(callerFilePath);
            if (DevelopmentDirectory is null)
            {
                Log.Error("Could not find project source.");
                return;
            }

            if (ShaderDirectory is not { Exists: true } || ShaderWatchers.ContainsKey(shaderName)) return;

            FileSystemWatcher watcher = new FileSystemWatcher(ShaderDirectory.FullName, $"{shaderName}.mgfx*");
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += (sender, e) =>
            {
                try
                {
                    (sender as FileSystemWatcher)!.EnableRaisingEvents = false;
                    Thread.Sleep(100);
                    byte[] stream = File.ReadAllBytes(e.FullPath);
                    Effect newEffect = new Effect(Game1.graphics.GraphicsDevice, stream);
                    onChange(newEffect);
                    Log.Success($"Shader {shaderName} reloaded successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to reload shader {shaderName}: {ex.Message}");
                }
                finally
                {
                    (sender as FileSystemWatcher)!.EnableRaisingEvents = true;
                }
            };

            watcher.EnableRaisingEvents = true;
            ShaderWatchers[shaderName] = watcher;

            WatchShaderSource(shaderName);
        }

        private static void WatchShaderSource(string shaderName)
        {
            if (FxDirectory is not { Exists: true } || SourceWatchers.ContainsKey(shaderName)) return;

            FileSystemWatcher sourceWatcher = new FileSystemWatcher(FxDirectory.FullName, $"{shaderName}.fx");
            sourceWatcher.NotifyFilter = NotifyFilters.LastWrite;
            sourceWatcher.Changed += (sender, e) =>
            {
                try
                {
                    (sender as FileSystemWatcher)!.EnableRaisingEvents = false;
                    Thread.Sleep(100);
                    string inputPath = e.FullPath;
                    string outputPath = Path.Combine(ShaderDirectory!.FullName, $"{shaderName}.mgfx");
                    var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "mgfxc",
                        Arguments = $"\"{inputPath}\" \"{outputPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    process?.WaitForExit();
                    string error = process?.StandardError.ReadToEnd() ?? "";
                    if (!string.IsNullOrEmpty(error)) throw new Exception(error);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to recompile shader source {shaderName}: {ex.Message}");
                }
                finally
                {
                    (sender as FileSystemWatcher)!.EnableRaisingEvents = true;
                }
            };
            sourceWatcher.EnableRaisingEvents = true;
            SourceWatchers[shaderName] = sourceWatcher;
        }

        private static DirectoryInfo? FindSourceDirectory(string? fileInSourcePath)
        {
            if (string.IsNullOrEmpty(fileInSourcePath)) return null;
            FileInfo fileInfo = new FileInfo(fileInSourcePath);
            DirectoryInfo? dir = fileInfo.Directory;
            while (dir != null && !dir.GetFiles("*.csproj").Any())
            {
                dir = dir.Parent;
            }

            return dir;
        }
    }
}