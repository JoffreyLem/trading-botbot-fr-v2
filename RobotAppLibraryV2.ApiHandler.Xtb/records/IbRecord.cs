using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class IbRecord : BaseResponseRecord
{
    public IbRecord()
    {
    }

    public IbRecord(JSONObject value)
    {
        FieldsFromJSONObject(value);
    }

    /// <summary>
    ///     IB close price or null if not allowed to view.
    /// </summary>
    public double ClosePrice { get; set; }

    /// <summary>
    ///     IB user login or null if not allowed to view.
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    ///     IB nominal or null if not allowed to view.
    /// </summary>
    public double Nominal { get; set; }

    /// <summary>
    ///     IB open price or null if not allowed to view.
    /// </summary>
    public double OpenPrice { get; set; }

    /// <summary>
    ///     Operation code or null if not allowed to view.
    /// </summary>
    public Side Side { get; set; }

    /// <summary>
    ///     IB user surname or null if not allowed to view.
    /// </summary>
    public string Surname { get; set; }

    /// <summary>
    ///     Symbol or null if not allowed to view.
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    ///     Time the record was created or null if not allowed to view.
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    ///     Volume in lots or null if not allowed to view.
    /// </summary>
    public double Volume { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        ClosePrice = (double)value["closePrice"];
        Login = (string)value["login"];
        Nominal = (double)value["nominal"];
        OpenPrice = (double)value["openPrice"];
        Side = Side.FromCode((int)value["side"]);
        Surname = (string)value["surname"];
        Symbol = (string)value["symbol"];
        Timestamp = (long)value["timestamp"];
        Volume = (double)value["volume"];
    }
}