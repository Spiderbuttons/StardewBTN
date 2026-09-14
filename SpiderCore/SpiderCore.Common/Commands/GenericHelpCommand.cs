using System.Linq;
using SpiderCore.Common.Logging;

namespace SpiderCore.Common.Commands
{
    public class GenericHelpCommand() : ConsoleCommand("help", allowOnTitle: true)
    {
        public override string GetDescription()
        {
            return $"{Handler.RootCommand} {Name}\r\n" +
                   $"   Provides information about {Handler.ModName} commands.\r\n" +
                   $"   Usage: {Handler.RootCommand} {Name}\r\n" +
                   $"      Lists all available {Handler.ModName} commands.\r\n" +
                   $"   Usage: {Handler.RootCommand} {Name} <command>\r\n" +
                   $"      Displays help for a specific command.\r\n" +
                   $"      <command> - The name of the command to display help for.";
        }

        public override void Handle(string[] args)
        {
            var commands = CommandHandler.Commands;
            string text;

            if (!args.Any())
            {
                text = $"The '{Handler.RootCommand}' command is the command prefix for {Handler.ModName} commands. To use a {Handler.ModName} command, type '{Handler.RootCommand}' followed by the command name and any applicable arguments.\r\n\r\n" + 
                       $"Available commands:";
                foreach (var command in commands.Values)
                {
                    text += $"\r\n\r\n" + $"{command.Description}";
                }
            } else if (commands.TryGetValue(args[0], out var command))
            {
                text = "\r\n" + command.GetDescription();
            }
            else
            {
                text = $"The '{Handler.RootCommand} {args[0]}' command is not a valid {Handler.ModName} command.";
            }

            Log.Info(text.TrimEnd());
        }
    }
}