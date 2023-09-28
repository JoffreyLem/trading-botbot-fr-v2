using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class IbsHistoryResponse : BaseResponse
{
    public IbsHistoryResponse(string body)
        : base(body)
    {
        var arr = (JSONArray)ReturnData;

        foreach (JSONObject e in arr)
        {
            var record = new IbRecord(e);
            IbRecords.AddLast(record);
        }
    }

    /// <summary>
    ///     IB records.
    /// </summary>
    public LinkedList<IbRecord> IbRecords { get; set; }
}