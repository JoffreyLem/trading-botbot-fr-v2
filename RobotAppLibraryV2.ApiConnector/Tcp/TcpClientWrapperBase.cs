using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using RobotAppLibraryV2.ApiConnector.Exceptions;
using Serilog;

namespace RobotAppLibraryV2.ApiConnector.Tcp;

public abstract class TcpClientWrapperBase : ITcpConnectorBase, IDisposable
{
    private readonly TcpClient client = new();

    protected readonly ILogger Logger;

    protected StreamReader? ApiReadStream;

    protected StreamWriter? ApiWriteStream;

    protected TimeSpan CommandTimeSpanmeSpace = TimeSpan.FromMilliseconds(200);

    public bool IsConnected => client.Connected;

    protected int Port;

    protected string ServerAddress;

    private SslStream stream;

    public TimeSpan TimeOutMilliSeconds = TimeSpan.FromMilliseconds(5000);

    protected TcpClientWrapperBase(string serverAddress, int port, ILogger logger)
    {
        Logger = logger.ForContext(GetType());
        ServerAddress = serverAddress;
        Port = port;
    }

    public void Dispose()
    {
        ApiReadStream?.Dispose();
        ApiWriteStream?.Dispose();
        client?.Dispose();
    }

    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public virtual async Task ConnectAsync()
    {
        try
        {
            if (IsConnected) return;
            var connectTask = client.ConnectAsync(ServerAddress, Port);
            var delayTask = Task.Delay(TimeOutMilliSeconds);

            var completedTask = await Task.WhenAny(connectTask, delayTask);

            if (completedTask == delayTask)
            {
                Close();
                throw new ApiCommunicationException("Connection timed out.");
            }

            await connectTask;
            stream = new SslStream(client.GetStream(), false, ValidateServerCertificate);
            var authenticationTask = stream.AuthenticateAsClientAsync(ServerAddress, new X509CertificateCollection(),
                SslProtocols.Tls13 | SslProtocols.Tls12, true);
            var delayTask2 = Task.Delay(TimeSpan.FromSeconds(30));

            var completedTask2 = await Task.WhenAny(authenticationTask, delayTask2);

            if (completedTask2 == delayTask) throw new TimeoutException("SSL handshake timed out.");
            var bufferedStream = new BufferedStream(stream, 8192);
            
            ApiWriteStream ??= new StreamWriter(bufferedStream,  leaveOpen: true);
            ApiReadStream ??= new StreamReader(bufferedStream, leaveOpen: true);
            OnConnectedEvent();
        }
        catch (Exception e)
        {
            Logger.Information(e, "Error on tcp connection");
            Close();
            throw new ApiCommunicationException("Error on connection", e);
        }
    }

    public virtual async Task SendAsync(string messageToSend)
    {
        if (!IsConnected)
        {
            Close();
            throw new ApiCommunicationException("Error while sending the data (socket disconnected)");
        }

        try
        {
            await ApiWriteStream.WriteAsync(messageToSend);
            await ApiWriteStream.FlushAsync();
        }
        catch (IOException ex)
        {
            Close();
            throw new ApiCommunicationException("Error while sending the data: " + ex.Message);
        }
   
    }

    public  Task<string> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var result = new StringBuilder();
        var lastChar = ' ';

        try
        {
            // var buffer = new byte[client.ReceiveBufferSize];
            string line;
            while ((line =  ApiReadStream.ReadLine()) != null)
            {
                result.Append(line);

                // Last line is always empty
                if (line == "" && lastChar == '}')
                    break;

                if (line.Length != 0) lastChar = line[^1];
            }
            
            return Task.FromResult(result.ToString());
        }
        catch (OperationCanceledException)
        {
            Close();
            throw new TimeoutException("The operation has timed out.");
        }
        catch (Exception ex)
        {
            Close();
            throw new ApiCommunicationException("Disconnected from server: " + ex.Message, ex);
        }
    }





    public void Close()
    {
        if (IsConnected)
        {
            ApiReadStream?.Close();
            ApiWriteStream?.Close();
            client.Close();
            OnDisconnected();
        }
    }

    protected void OnConnectedEvent()
    {
        Logger.Information("{Connector} Connected to {server}:{port}", GetType().Name, ServerAddress, Port);
        Connected?.Invoke(this, EventArgs.Empty);
    }

    protected void OnDisconnected()
    {
        Logger.Information("{Connector} Disconnected from {server}:{port}", GetType().Name, ServerAddress, Port);
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        Logger.Error("Certificate error: {0}", sslPolicyErrors);

        return false;
    }
}