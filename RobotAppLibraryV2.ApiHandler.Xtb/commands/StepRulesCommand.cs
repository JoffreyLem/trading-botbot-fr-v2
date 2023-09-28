using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class StepRulesCommand : BaseCommand
{
    public StepRulesCommand()
        : base(new JSONObject(), false)
    {
    }

    public override string CommandName => "getStepRules";

    public override string[] RequiredArguments
    {
        get { return new string[] { }; }
    }
}