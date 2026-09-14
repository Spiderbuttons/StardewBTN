using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpiderCore.Common.Logging;
using StardewModdingAPI;

namespace SpiderCore.Common.Commands
{
    public class CommandHandler(IModHelper helper, IManifest manifest, string rootCommand)
    {
        private ICommandHelper CommandHelper { get; } = helper.ConsoleCommands;
        public string ModName { get; } = manifest.Name;
        public string RootCommand { get; } = rootCommand;

        public static Dictionary<string, ConsoleCommand> Commands { get; } = new();

        public void Register()
        {
            var comms = typeof(CommandHandler).Assembly.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false } && type.GetCustomAttribute(typeof(CommandAttribute)) != null);
            foreach (var type in comms)
            {
                var instance = (ConsoleCommand)Activator.CreateInstance(type)!;
                FieldInfo handlerField = type.GetField("Handler", BindingFlags.NonPublic | BindingFlags.Instance)!;
                handlerField.SetValue(instance, this);
                Commands.Add(instance.Name, instance);
            }
        
            CommandHelper.Add(RootCommand, $"Starts a {ModName} command.", (_, args) => Handle(args));
        }

        private void Handle(string[] args)
        {
            string command = args.FirstOrDefault() ?? "help";
            string[] commandArgs = args.Length > 1 ? args.Skip(1).ToArray() : [];
            if (Commands.TryGetValue(command, out var handler))
            {
                if (!handler.AllowOnTitle && !Context.IsWorldReady)
                {
                    Log.Error($"The '{RootCommand} {command}' command can only be used when a save is loaded.");
                    return;
                }
            
                handler.Handle(commandArgs);
                return;
            }
            Log.Error($"The '{RootCommand} {command}' command is not a valid {ModName} command.");
        }
    }
}