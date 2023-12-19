using System.Text.Json;
using RobotAppLibraryV2.ApiConnector.Exceptions;
using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.ApiConnector.Modeles;
using RobotAppLibraryV2.ApiConnector.Tcp;
using Serilog;

namespace RobotAppLibraryV2.Api.Xtb;

public class StreamingClientXtb : TcpStreamingConnector
{
    private readonly IReponseAdapter adapter;

    public StreamingClientXtb(Server server, ILogger logger, XtbAdapter adapter) : base(server, logger)
    {
        this.adapter = adapter;
    }

    protected override void HandleMessage(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;
            var commandName = root.GetProperty("command").GetString();

            switch (commandName)
            {
                case "tickPrices":
                    OnTickRecordReceived(adapter.AdaptTickRecordStreaming(message));
                    break;
                case "trade":
                    OnTradeRecordReceived(adapter.AdaptTradeRecordStreaming(message));
                    break;
                case "balance":
                    OnBalanceRecordReceived(adapter.AdaptBalanceRecordStreaming(message));
                    break;
                case "tradeStatus":
                    OnTradeRecordReceived(adapter.AdaptTradeStatusRecordStreaming(message));
                    break;
                case "profit":
                    OnProfitRecordReceived(adapter.AdaptProfitRecordStreaming(message));
                    break;
                case "news":
                    OnNewsRecordReceived(adapter.AdaptNewsRecordStreaming(message));
                    break;
                case "keepAlive":
                    OnKeepAliveRecordReceived();
                    break;
                case "candle":
                    OnCandleRecordReceived(adapter.AdaptCandleRecordStreaming(message));
                    break;
                default:
                    throw new ApiCommunicationException("Unknown streaming record received");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "error on message reception {@message}", message);
        }
    }
}