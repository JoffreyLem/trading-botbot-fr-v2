using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class IbsHistoryCommand : BaseCommand
{
    public IbsHistoryCommand(JSONObject arguments, bool prettyPrint)
        : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "getIbsHistory";

    public override string[] RequiredArguments
    {
        get { return new[] { "start", "end" }; }
    }
}