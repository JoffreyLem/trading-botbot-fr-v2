using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class MarginTradeCommand : BaseCommand
{
    public MarginTradeCommand(JSONObject arguments, bool prettyPrint) : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "getMarginTrade";

    public override string[] RequiredArguments
    {
        get { return new[] { "symbol", "volume" }; }
    }
}