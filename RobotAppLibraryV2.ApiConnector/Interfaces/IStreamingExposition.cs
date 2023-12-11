using RobotAppLibraryV2.ApiConnector.Tcp;

namespace RobotAppLibraryV2.ApiConnector.Interfaces;

public interface ITcpExposition
{
    ITcpStreamingConnector TcpStreamingConnector { get; }

    ITcpConnectorSynchronisation TcpConnector { get; }
}