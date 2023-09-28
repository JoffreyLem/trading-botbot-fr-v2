using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public class TradeTransactionStatusCommand : BaseCommand
{
    public TradeTransactionStatusCommand(JSONObject arguments, bool prettyPrint)
        : base(arguments, prettyPrint)
    {
    }

    public override string CommandName => "tradeTransactionStatus";

    public override string[] RequiredArguments
    {
        get { return new[] { "order" }; }
    }
}