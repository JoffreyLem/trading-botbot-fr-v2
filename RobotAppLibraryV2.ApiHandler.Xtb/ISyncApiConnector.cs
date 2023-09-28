using RobotAppLibraryV2.ApiHandler.Xtb.commands;
using RobotAppLibraryV2.ApiHandler.Xtb.sync;
using JSONObject = Newtonsoft.Json.Linq.JObject;

namespace RobotAppLibraryV2.ApiHandler.Xtb;

public interface ISyncApiConnector : IConnector
{
    public delegate void OnConnectedCallback(Server server);

    public delegate void OnDisconnectCallback();


    public delegate void OnRedirectedCallback(Server server);


    public const string VERSION = "2.5.0";


    private const long COMMAND_TIME_SPACE = 200;


    public const long MAX_REDIRECTS = 3;


    private const int TIMEOUT = 5000;

    string StreamSessionId { get; set; }

    IStreamingApiConnector StreamingApiConnector { get; }

    void Connect();

    void Redirect(Server server);

    JSONObject ExecuteCommand(BaseCommand cmd);

    string ExecuteCommand(string message);

    /// <summary>
    ///     Event raised when connection is established.
    /// </summary>
    public event OnConnectedCallback OnConnected;


    public event OnDisconnectCallback OnDisconnectedCallBack;

    /// <summary>
    ///     Event raised when connection is redirected.
    /// </summary>
    public event OnRedirectedCallback OnRedirected;
}