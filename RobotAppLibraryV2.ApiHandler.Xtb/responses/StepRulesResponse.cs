using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class StepRulesResponse : BaseResponse
{
    private readonly LinkedList<StepRuleRecord> stepRulesRecords = new();

    public StepRulesResponse(string body)
        : base(body)
    {
        var stepRulesRecords = (JSONArray)ReturnData;
        foreach (JSONObject e in stepRulesRecords)
        {
            var stepRulesRecord = new StepRuleRecord();
            stepRulesRecord.FieldsFromJSONObject(e);
            this.stepRulesRecords.AddLast(stepRulesRecord);
        }
    }

    public virtual LinkedList<StepRuleRecord> StepRulesRecords => stepRulesRecords;
}