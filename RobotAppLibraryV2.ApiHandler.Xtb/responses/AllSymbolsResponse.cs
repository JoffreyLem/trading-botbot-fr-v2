using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class AllSymbolsResponse : BaseResponse
{
    private readonly LinkedList<SymbolRecord> symbolRecords = new();

    public AllSymbolsResponse(string body) : base(body)
    {
        var symbolRecords = (JSONArray)ReturnData;
        foreach (JSONObject e in symbolRecords)
        {
            var symbolRecord = new SymbolRecord();
            symbolRecord.FieldsFromJSONObject(e);
            this.symbolRecords.AddLast(symbolRecord);
        }
    }
    
    public AllSymbolsResponse(JSONObject jsonBody) : base(jsonBody)
    {
        if (jsonBody.TryGetValue("returnData", out var returnDataObject) && returnDataObject is JSONArray symbolRecordsArray)
        {
            foreach (JSONObject e in symbolRecordsArray)
            {
                var symbolRecord = new SymbolRecord();
                symbolRecord.FieldsFromJSONObject(e);
                symbolRecords.AddLast(symbolRecord);
            }
        }
    }

    public virtual LinkedList<SymbolRecord> SymbolRecords => symbolRecords;
}