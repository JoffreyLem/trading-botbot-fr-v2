using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class LogoutCommand : BaseCommand
{
    public LogoutCommand() : base(new JSONObject(), false)
    {
    }

    public override string CommandName => "logout";

    public override string[] RequiredArguments
    {
        get { return new string[] { }; }
    }

    public override string ToJSONString()
    {
        var obj = new JSONObject();
        obj.Add("command", commandName);
        return obj.ToString();
    }
}