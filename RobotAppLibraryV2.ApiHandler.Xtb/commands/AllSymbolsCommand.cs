namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

public class AllSymbolsCommand : BaseCommand
{
    public AllSymbolsCommand(bool prettyPrint) : base(prettyPrint)
    {
    }

    public override string CommandName => "getAllSymbols";

    public override string[] RequiredArguments
    {
        get { return new string[] { }; }
    }
}