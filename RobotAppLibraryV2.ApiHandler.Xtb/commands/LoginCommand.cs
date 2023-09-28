using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class LoginCommand : BaseCommand
{
    public LoginCommand(JSONObject arguments, bool prettyPrint) : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "login";

    public override string[] RequiredArguments
    {
        get { return new[] { "userId", "password" }; }
    }
}