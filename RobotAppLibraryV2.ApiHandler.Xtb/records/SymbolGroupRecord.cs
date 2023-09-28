using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class SymbolGroupRecord : BaseResponseRecord
{
    private string description;
    private string name;
    private long? type;

    [Obsolete("Command getAllSymbolGroups is not available in API any more")]
    public SymbolGroupRecord()
    {
    }

    public virtual long? Type => type;

    public virtual string Description => description;

    public virtual string Name => name;

    public void FieldsFromJSONObject(JSONObject value)
    {
        type = (long?)value["type"];
        description = (string)value["description"];
        name = (string)value["name"];
    }
}