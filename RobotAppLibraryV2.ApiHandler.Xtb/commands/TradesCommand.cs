using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class TradesCommand : BaseCommand
{
    public TradesCommand(JSONObject arguments, bool prettyPrint)
        : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "getTrades";

    public override string[] RequiredArguments
    {
        get { return new[] { "openedOnly" }; }
    }
}