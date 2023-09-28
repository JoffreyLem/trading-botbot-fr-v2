using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class MarginLevelCommand : BaseCommand
{
    public MarginLevelCommand(bool? prettyPrint) : base(new JSONObject(), prettyPrint)
    {
    }

    public override string CommandName => "getMarginLevel";

    public override string[] RequiredArguments
    {
        get { return new string[] { }; }
    }
}