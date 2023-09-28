using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using RobotAppLibraryV2.ApiHandler.Xtb.commands;
using RobotAppLibraryV2.ApiHandler.Xtb.errors;
using RobotAppLibraryV2.ApiHandler.Xtb.utils;
using JSONObject = Newtonsoft.Json.Linq.JObject;

namespace RobotAppLibraryV2.ApiHandler.Xtb.sync;

public class SyncAPIConnector : Connector, ISyncApiConnector
{
    /// <summary>
    ///     Lock object used to synchronize access to read/write socket operations.
    /// </summary>
    private readonly object locker = new();

    /// <summary>
    ///     Last command timestamp (used to calculate interval between each command).
    /// </summary>
    private long lastCommandTimestamp;

    /// <summary>
    ///     Creates new ISyncAPIConnector instance based on given Server data.
    /// </summary>
    /// <param name="server">Server data</param>
    /// <param name="lookForBackups">If false, no connection to backup servers will be made</param>
    public SyncAPIConnector(Server server, bool lookForBackups = true)
    {
        Connect(server, lookForBackups);
    }


    /// <summary>
    ///     Streaming API connector.
    /// </summary>
    public IStreamingApiConnector StreamingApiConnector { get; private set; }

    /// <summary>
    ///     Connects to the remote server (NOTE: server must be already set).
    /// </summary>
    public void Connect()
    {
        if (server != null)
            Connect(server);
        else
            throw new APICommunicationException("No server to connect to");
    }


    /// <summary>
    ///     Redirects to the given server.
    /// </summary>
    /// <param name="server">Server data</param>
    public void Redirect(Server server)
    {
        if (OnRedirected != null) OnRedirected.Invoke(server);

        if (apiConnected) Disconnect(true);

        Connect(server);
    }

    /// <summary>
    ///     Executes given command and receives response (withholding API inter-command timeout).
    /// </summary>
    /// <param name="cmd">Command to execute</param>
    /// <returns>Response from the server</returns>
    public JSONObject ExecuteCommand(BaseCommand cmd)
    {
        try
        {
            return JSONObject.Parse(ExecuteCommand(cmd.ToJSONString()));
        }
        catch (Exception ex)
        {
            throw new APICommunicationException("Problem with executing command: " + ex.Message);
        }
    }

    /// <summary>
    ///     Executes given command and receives response (withholding API inter-command timeout).
    /// </summary>
    /// <param name="message">Command to execute</param>
    /// <returns>Response from the server</returns>
    public string ExecuteCommand(string message)
    {
        lock (locker)
        {
            var currentTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var interval = currentTimestamp - lastCommandTimestamp;

            // If interval between now and last command is less than minimum command time space - wait
            if (interval < COMMAND_TIME_SPACE) Thread.Sleep((int)(COMMAND_TIME_SPACE - interval));

            WriteMessage(message);

            lastCommandTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var response = ReadMessage();

            if (response == null || response.Equals(""))
            {
                Disconnect();
                throw new APICommunicationException("Server not responding");
            }

            return response;
        }
    }

    public event ISyncApiConnector.OnConnectedCallback? OnConnected;

    /// <summary>
    ///     Stream session id (given upon login).
    /// </summary>
    public string StreamSessionId { get; set; }

    /// <summary>
    ///     Connects to the remote server.
    /// </summary>
    /// <param name="server">Server data</param>
    /// <param name="lookForBackups">If false, no connection to backup servers will be made</param>
    private void Connect(Server server, bool lookForBackups = true)
    {
        this.server = server;
        apiSocket = new TcpClient();

        var connectionAttempted = false;

        while (!connectionAttempted || !apiSocket.Connected)
        {
            // Try to connect asynchronously and wait for the result
            var result = apiSocket.BeginConnect(this.server.Address, this.server.MainPort, null, null);
            connectionAttempted = result.AsyncWaitHandle.WaitOne(TIMEOUT, true);

            // If connection attempt failed (timeout) or not connected
            if (!connectionAttempted || !apiSocket.Connected)
            {
                apiSocket.Close();
                if (lookForBackups)
                {
                    this.server = Servers.GetBackup(this.server);
                    apiSocket = new TcpClient();
                }
                else
                {
                    throw new APICommunicationException("Cannot connect to: " + server.Address + ":" + server.MainPort);
                }
            }
        }

        if (server.Secure)
        {
            var sl = new SslStream(apiSocket.GetStream(), false, SSLHelper.TrustAllCertificatesCallback);

            //sl.AuthenticateAsClient(server.Address);

            var authenticated = ExecuteWithTimeLimit.Execute(TimeSpan.FromMilliseconds(5000),
                () =>
                {
                    sl.AuthenticateAsClient(server.Address, new X509CertificateCollection(), SslProtocols.Tls13,
                        false);
                });

            if (!authenticated) throw new APICommunicationException("Error during SSL handshaking (timed out?)");

            apiWriteStream = new StreamWriter(sl);
            apiReadStream = new StreamReader(sl);
        }
        else
        {
            var ns = apiSocket.GetStream();
            apiWriteStream = new StreamWriter(ns);
            apiReadStream = new StreamReader(ns);
        }

        apiConnected = true;

        if (OnConnected != null) OnConnected.Invoke(this.server);


        StreamingApiConnector = new StreamingAPIConnector(this.server);
    }

    #region Settings

    /// <summary>
    ///     Wrappers version.
    /// </summary>
    public const string VERSION = "2.5.0";

    /// <summary>
    ///     Delay between each command to the server.
    /// </summary>
    private const long COMMAND_TIME_SPACE = 200;

    /// <summary>
    ///     Maximum number of redirects (to avoid redirection loops).
    /// </summary>
    public const long MAX_REDIRECTS = 3;

    /// <summary>
    ///     Maximum connection time (in milliseconds). After that the connection attempt is immidiately dropped.
    /// </summary>
    private const int TIMEOUT = 5000;

    #endregion

    #region Events

    /// <summary>
    ///     Delegate called on connection establish.
    /// </summary>
    /// <param name="server">Server that the connection was made to</param>
    public delegate void OnConnectedCallback(Server server);

    /// <summary>
    ///     Event raised when connection is established.
    /// </summary>
    public event ISyncApiConnector.OnDisconnectCallback? OnDisconnectedCallBack;

    public event ISyncApiConnector.OnRedirectedCallback? OnRedirected;


    /// <summary>
    ///     Delegate called on connection redirect.
    /// </summary>
    /// <param name="server">Server that the connection was made to</param>
    public delegate void OnRedirectedCallback(Server server);

    /// <summary>
    /// Event raised when connection is redirected.
    /// </summary>

    #endregion
}