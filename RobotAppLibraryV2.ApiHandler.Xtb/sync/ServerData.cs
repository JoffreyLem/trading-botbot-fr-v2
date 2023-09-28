namespace RobotAppLibraryV2.ApiHandler.Xtb.sync;

[Obsolete("Use Servers class instead")]
public class ServerData
{
    private static readonly string XAPI_A = "xapi.xtb.com";
    private static readonly string XAPI_B = "xapi.xtb.com";

    private static readonly int[] PORTS_REAL = { 5112, 5113 };
    private static readonly int[] PORTS_DEMO = { 5124, 5125 };

    private static Dictionary<string, string> xapiList;

    public ServerData()
    {
        SetUpList();
    }

    /// <summary>
    ///     Static method which receives map of production servers
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, Server> ProductionServers
    {
        get
        {
            var dict = new Dictionary<string, Server>();

            dict = AddServers(dict, PORTS_DEMO, "DEMO");
            dict = AddServers(dict, PORTS_REAL, "REAL");

            return dict;
        }
    }

    private static void SetUpList()
    {
        xapiList = new Dictionary<string, string>();
        xapiList.Add("A", XAPI_A);
        xapiList.Add("B", XAPI_B);
    }

    /// <summary>
    ///     Static method that adds a server
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="portsArray"></param>
    /// <param name="desc"></param>
    /// <returns></returns>
    private static Dictionary<string, Server> AddServers(Dictionary<string, Server> dict, int[] portsArray, string desc)
    {
        if (xapiList == null) SetUpList();

        var mainPort = portsArray[0];
        var streamingPort = portsArray[1];

        foreach (var xapiKey in xapiList.Keys)
        {
            var address = xapiList[xapiKey];
            var dictKey = "XSERVER_" + desc + "_" + xapiKey;
            var dictDesc = "xServer " + desc + " " + xapiKey;
            dict.Add(dictKey, new Server(address, mainPort, streamingPort, true, dictDesc));
        }

        return dict;
    }
}