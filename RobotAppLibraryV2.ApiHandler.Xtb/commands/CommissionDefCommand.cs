using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class CommissionDefCommand : BaseCommand
{
    public CommissionDefCommand(JSONObject arguments, bool prettyPrint) : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "getCommissionDef";

    public override string[] RequiredArguments
    {
        get { return new[] { "symbol", "volume" }; }
    }

    public override string ToJSONString()
    {
        var obj = new JSONObject();
        obj.Add("command", commandName);
        obj.Add("prettyPrint", prettyPrint);
        obj.Add("arguments", arguments);
        obj.Add("extended", true);
        return obj.ToString();
    }
}