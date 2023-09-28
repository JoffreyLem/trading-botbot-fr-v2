using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class CalendarCommand : BaseCommand
{
    public CalendarCommand(bool prettyPrint)
        : base(new JSONObject(), prettyPrint)
    {
    }

    public override string CommandName => "getCalendar";

    public override string[] RequiredArguments
    {
        get { return new string[] { }; }
    }
}