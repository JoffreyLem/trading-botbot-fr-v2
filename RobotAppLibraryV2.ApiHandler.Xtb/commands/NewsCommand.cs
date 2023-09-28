using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class NewsCommand : BaseCommand
{
    public NewsCommand(JSONObject body, bool prettyPrint)
        : base(body, prettyPrint)
    {
    }

    public override string CommandName => "getNews";

    public override string[] RequiredArguments
    {
        get { return new[] { "start", "end" }; }
    }
}