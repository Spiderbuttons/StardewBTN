namespace SpiderCore.Common.Commands;

[Command]
public abstract class ConsoleCommand(string name, bool allowOnTitle = false)
{
    protected readonly CommandHandler Handler = null!;
    
    public readonly string Name = name;
    public readonly bool AllowOnTitle = allowOnTitle;
    public string Description => GetDescription();

    public abstract string GetDescription();
    public abstract void Handle(string[] args);
}