using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class SymbolResponse : BaseResponse
{
    private readonly SymbolRecord symbol;

    public SymbolResponse(string body) : base(body)
    {
        var ob = (JSONObject)ReturnData;
        symbol = new SymbolRecord();
        symbol.FieldsFromJSONObject(ob);
    }

    public virtual SymbolRecord Symbol => symbol;
}