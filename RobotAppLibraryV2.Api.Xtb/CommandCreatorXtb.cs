using System.Text;
using System.Text.Json;
using RobotAppLibraryV2.Api.Xtb.Assembler;
using RobotAppLibraryV2.Api.Xtb.Code;
using RobotAppLibraryV2.Api.Xtb.Utils;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;

namespace RobotAppLibraryV2.Api.Xtb;

public class CommandCreatorXtb : ICommandCreatorXtb
{
    private string? streamingSessionId;

    public string? StreamingSessionId
    {
        get
        {
            if (string.IsNullOrEmpty(streamingSessionId))
                throw new ArgumentException("The streaming session id is empty");

            return streamingSessionId;
        }
        set => streamingSessionId = value;
    }

    public string CreateLoginCommand(Credentials credentials)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("userId", credentials.User);
        writer.WriteString("password", credentials.Password);
        writer.WriteString("type", "dotNET");
        writer.WriteEndObject();
        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());
        var args = doc.RootElement;

        return WriteBaseCommand("login", args);
    }

    public string CreateAllSymbolsCommand()
    {
        return WriteBaseCommand("getAllSymbols", null);
    }

    public string CreateCalendarCommand()
    {
        return WriteBaseCommand("getCalendar", null);
    }

    public string CreateFullChartCommand(Timeframe timeframe, DateTime start, string symbol)
    {
        var chartLastInfoRecordJson = JsonSerializer.Serialize(new
        {
            symbol,
            period = ToXtbAssembler.ToPeriodCode(timeframe),

            start = SetDateTime(timeframe).ConvertToUnixTime()
        });

        var fullJson = $"{{\"arguments\": {{\"info\": {chartLastInfoRecordJson}}}}}";

        using var doc = JsonDocument.Parse(fullJson);
        var argumentsElement = doc.RootElement.GetProperty("arguments");

        return WriteBaseCommand("getChartLastRequest", argumentsElement);
    }

    public string CreateRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end, string symbol)
    {
        var chartLastInfoRecordJson = JsonSerializer.Serialize(new
        {
            symbol,
            period = ToXtbAssembler.ToPeriodCode(timeframe),
            start = start.ConvertToUnixTime(),
            end = end.ConvertToUnixTime()
        });

        var fullJson = $"{{\"arguments\": {{\"info\": {chartLastInfoRecordJson}}}}}";

        using var doc = JsonDocument.Parse(fullJson);
        var argumentsElement = doc.RootElement.GetProperty("arguments");

        return WriteBaseCommand("getChartRangeRequest", argumentsElement);
    }

    public string CreateLogOutCommand()
    {
        return WriteBaseCommand("logout", null);
    }

    public string CreateBalanceAccountCommand()
    {
        return WriteBaseCommand("getMarginLevel", null);
    }

    public string CreateNewsCommand(DateTime? start, DateTime? end)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteNumber("start", start.GetValueOrDefault().ConvertToUnixTime());
        writer.WriteNumber("end", end.GetValueOrDefault().ConvertToUnixTime());
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("", doc.RootElement);
    }


    public string CreateCurrentUserDataCommand()
    {
        return WriteBaseCommand("getCurrentUserData", null);
    }

    public string CreatePingCommand()
    {
        return WriteBaseCommand("ping", null);
    }

    public string CreateSymbolCommand(string symbol)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("symbol", symbol);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getSymbol", doc.RootElement);
    }

    public string CreateTickCommand(string symbol)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteStartArray("symbols");
        writer.WriteStringValue(symbol);
        writer.WriteEndArray();
        writer.WriteNumber("timestamp", 0);
        writer.WriteNumber("level", 0);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTickPrices", doc.RootElement);
    }

    public string CreateTradesHistoryCommand()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();

        writer.WriteNumber("start", new DateTime(2000, 01, 01).ConvertToUnixTime());
        writer.WriteNumber("end", 0);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTradesHistory", doc.RootElement);
    }

    public string CreateTradesOpenedTradesCommand()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteBoolean("openedOnly", true);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTrades", doc.RootElement);
        ;
    }

    public string CreateTradingHoursCommand(string symbol)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteStartArray("symbols");
        writer.WriteStringValue(symbol);
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTradingHours", doc.RootElement);
    }

    public string CreateOpenTradeCommande(Position position, decimal price)
    {
        return CreateTradeTransactionCommand(position, price, TRADE_TRANSACTION_TYPE.ORDER_OPEN.Code);
    }

    public string CreateUpdateTradeCommande(Position position, decimal price)
    {
        return CreateTradeTransactionCommand(position, price, TRADE_TRANSACTION_TYPE.ORDER_MODIFY.Code);
    }

    public string CreateCloseTradeCommande(Position position, decimal price)
    {
        return CreateTradeTransactionCommand(position, price, TRADE_TRANSACTION_TYPE.ORDER_CLOSE.Code);
    }

    public string CreateSubscribeBalanceCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getBalance");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopBalanceCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopBalance");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeCandleCommandStreaming(string symbol)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getCandles");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteString("symbol", symbol);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopCandleCommandStreaming(string symbol)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getCandles");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteString("symbol", symbol);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeKeepAliveCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getKeepAlive");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopKeepAliveCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopKeepAlive");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeNewsCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getNews");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopNewsCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopNews");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeProfitsCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getProfits");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopProfitsCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopProfits");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateTickPricesCommandStreaming(string symbol)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getTickPrices");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteString("symbol", symbol);
            writer.WriteNumber("minArrivalTime", 0);
            writer.WriteNumber("maxLevel", 0);

            writer.WriteEndObject();
            writer.Flush();
        }


        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopTickPriceCommandStreaming(string symbol)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopTickPrices");
            writer.WriteString("symbol", symbol);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateTradesCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getTrades");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopTradesCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopTrades");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateTradeStatusCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getTradeStatus");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopTradeStatusCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopTradeStatus");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreatePingCommandStreaming()
    {
        var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "ping");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopPingCommandStreaming()
    {
        throw new NotImplementedException();
    }

    private DateTime SetDateTime(Timeframe tf)
    {
        DateTime dateTime;

        switch (tf)
        {
            case Timeframe.OneMinute:
                dateTime = DateTime.Now.AddMonths(-1);
                return dateTime;
            case Timeframe.FiveMinutes:
                dateTime = DateTime.Now.AddMonths(-1);
                return dateTime;
            case Timeframe.FifteenMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.ThirtyMinutes:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.OneHour:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.FourHour:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.Daily:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.Weekly:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;
            case Timeframe.Monthly:
                dateTime = DateTime.Now.AddMonths(-7);
                return dateTime;


            default:
                throw new ArgumentException("Periode code n'existe pas");
        }
    }

    private string CreateTradeTransactionCommand(Position position, decimal price, long? typeCode)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var order = position.Order?.Split('|');

        writer.WriteStartObject();
        writer.WriteStartObject("tradeTransInfo");

        writer.WriteNumber("cmd", ToXtbAssembler.ToTradeOperationCode(position.TypePosition));
        writer.WriteNumber("type", typeCode.GetValueOrDefault());
        writer.WriteNumber("price", price);
        writer.WriteNumber("sl", position.StopLoss);
        writer.WriteNumber("tp", position.TakeProfit);
        writer.WriteString("symbol", position.Symbol);
        writer.WriteNumber("volume", position.Volume);
        writer.WriteNumber("order",
            typeCode != TRADE_TRANSACTION_TYPE.ORDER_OPEN.Code ? long.Parse(order[0]) : 0);
        writer.WriteString("customComment", position.PositionStrategyReferenceId);
        writer.WriteNumber("expiration", 0);

        writer.WriteEndObject();
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("tradeTransaction", doc.RootElement);
    }

    private string WriteBaseCommand(string commandName, JsonElement? arguments, bool prettyPrint = true)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("command", commandName);
        writer.WriteBoolean("prettyPrint", prettyPrint);

        if (arguments.HasValue && arguments.Value.ValueKind != JsonValueKind.Undefined &&
            arguments.Value.ValueKind != JsonValueKind.Null)
        {
            writer.WriteStartObject("arguments");
            foreach (var property in arguments.Value.EnumerateObject()) property.WriteTo(writer);
            writer.WriteEndObject();
        }

        writer.WriteString("customTag", CustomTagUtils.Next());
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}