using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class SymbolCommand : BaseCommand
{
    public SymbolCommand(JSONObject arguments, bool prettyPrint) : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "getSymbol";

    public override string[] RequiredArguments
    {
        get { return new[] { "symbol" }; }
    }
}