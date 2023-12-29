using System.Text.Json;
using RobotAppLibraryV2.Api.Xtb.Assembler;
using RobotAppLibraryV2.Api.Xtb.Code;
using RobotAppLibraryV2.ApiConnector.Exceptions;
using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.Modeles;
using RobotAppLibraryV2.Utils;

namespace RobotAppLibraryV2.Api.Xtb;

public class XtbAdapter : IReponseAdapter
{
    public List<SymbolInfo> AdaptAllSymbolsResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var symbolRecords = new List<SymbolInfo>();

        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Array)
            foreach (var symbolElement in returnData.Value.EnumerateArray())
            {
                var symbolRecord = new SymbolInfo();

                symbolRecord
                    .WithCategory(
                        FromXtbToRobotAssembler.GetCategory(symbolElement.GetProperty("categoryName").GetString()))
                    .WithContractSize(symbolElement.GetProperty("contractSize").GetInt64())
                    .WithCurrencyPair(symbolElement.GetProperty("currencyPair").GetBoolean())
                    .WithCurrency(symbolElement.GetProperty("currency").GetString())
                    .WithCurrencyProfit(symbolElement.GetProperty("currencyProfit").GetString())
                    .WithLotMax(symbolElement.GetProperty("lotMax").GetDouble())
                    .WithLotMin(symbolElement.GetProperty("lotMin").GetDouble())
                    .WithPrecision(symbolElement.GetProperty("precision").GetInt64())
                    .WithSymbol(symbolElement.GetProperty("symbol").GetString())
                    .WithTickSize(symbolElement.GetProperty("tickSize").GetDouble())
                    .WithLeverage(symbolElement.GetProperty("leverage").GetDouble());


                symbolRecords.Add(symbolRecord);
            }

        return symbolRecords;
    }


    public List<CalendarData> AdaptCalendarResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var calendarList = new List<CalendarData>();

        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Array)
            foreach (var calendarElement in returnData.Value.EnumerateArray())
            {
                var calendarEntry = new CalendarData
                {
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(calendarElement.GetProperty("time").GetInt64())
                        .DateTime,
                    Country = calendarElement.GetProperty("country").GetString(),
                    Title = calendarElement.GetProperty("title").GetString(),
                    Current = calendarElement.GetProperty("current").GetString(),
                    Previous = calendarElement.GetProperty("previous").GetString(),
                    Forecast = calendarElement.GetProperty("forecast").GetString(),
                    Impact = calendarElement.GetProperty("impact").GetString(),
                    Period = calendarElement.GetProperty("period").GetString()
                };

                calendarList.Add(calendarEntry);
            }

        return calendarList;
    }

    public List<Candle> AdaptFullChartResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var dataRecordsList = new List<Candle>();
        var digits = returnData.Value.GetProperty("digits").GetInt32();
        if (returnData.HasValue)
            foreach (var recordElement in returnData.Value.GetProperty("rateInfos").EnumerateArray())
                dataRecordsList.Add(MapCandle(recordElement, digits));

        dataRecordsList.Sort((c1, c2) => c1.Date.CompareTo(c2.Date));
        
        return dataRecordsList;
    }

    public List<Candle> AdaptRangeChartResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var dataRecordsList = new List<Candle>();
        var digits = returnData.Value.GetProperty("digits").GetInt32();
        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Array)
            foreach (var recordElement in returnData.Value.EnumerateArray())
                dataRecordsList.Add(MapCandle(recordElement, digits));

        return dataRecordsList;
    }


    // TODO : voir pour peut être changer ? 
    public string AdaptLogOutResponse(string jsonResponse)
    {
        return "";
    }

    public AccountBalance? AdaptBalanceAccountResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        return MapAccountBalance(returnData.Value);
    }


    public List<News> AdaptNewsResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);
        var data = new List<News>();

        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Array)
            foreach (var recordElement in returnData.Value.EnumerateArray())
                data.Add(MapNews(recordElement));

        return data;
    }


    public string AdaptCurrentUserDataResponse(string jsonResponse)
    {
        throw new NotImplementedException();
    }

    public bool AdaptPingResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        return true;
    }

    public SymbolInfo AdaptSymbolResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var symbolRecord = new SymbolInfo();

        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Object)
            symbolRecord
                .WithCategory(
                    FromXtbToRobotAssembler.GetCategory(returnData.Value.GetProperty("categoryName").GetString()))
                .WithContractSize(returnData.Value.GetProperty("contractSize").GetInt64())
                .WithCurrencyPair(returnData.Value.GetProperty("currencyPair").GetBoolean())
                .WithCurrency(returnData.Value.GetProperty("currency").GetString())
                .WithCurrencyProfit(returnData.Value.GetProperty("currencyProfit").GetString())
                .WithLotMax(returnData.Value.GetProperty("lotMax").GetDouble())
                .WithLotMin(returnData.Value.GetProperty("lotMin").GetDouble())
                .WithPrecision(returnData.Value.GetProperty("precision").GetInt64())
                .WithSymbol(returnData.Value.GetProperty("symbol").GetString())
                .WithTickSize(returnData.Value.GetProperty("tickSize").GetDouble())
                .WithLeverage(returnData.Value.GetProperty("leverage").GetDouble());


        return symbolRecord;
    }

    public Tick AdaptTickResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        if (returnData.HasValue &&
            returnData.Value.TryGetProperty("quotations", out var quotations) &&
            quotations.GetArrayLength() > 0)
        {
            var firstQuotation = quotations[0];

            return MapTick(firstQuotation);
        }

        return new Tick();
    }

    public List<Position?> AdaptTradesHistoryResponse(string jsonResponse, string positionReference)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var listPosition = new List<Position?>();

        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Array)
            foreach (var recordElement in returnData.Value.EnumerateArray())
                if (recordElement.GetProperty("customComment").GetString().Contains(positionReference))
                {
                    var position = MapPosition(recordElement);
                    listPosition.Add(position);
                }

        return listPosition;
    }

    public Position? AdaptTradesOpenedTradesResponse(string jsonResponse, string positionId)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var listPosition = new List<Position?>();

        if (returnData.HasValue && returnData.Value.ValueKind == JsonValueKind.Array)
            foreach (var recordElement in returnData.Value.EnumerateArray())
                if (recordElement.GetProperty("customComment").GetString().Contains(positionId))
                {
                    var position = MapPosition(recordElement);
                    // TODO : Faire évoluer ce truc bouche trou 
                    position.StatusPosition = StatusPosition.Open;
                    listPosition.Add(position);
                }

        return listPosition.FirstOrDefault();
    }

    public TradeHourRecord AdaptTradingHoursResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnDataElement = ReturnData(doc).Value;
        var firstData = returnDataElement.EnumerateArray().First();

        var tradeHourRecord = new TradeHourRecord();
        foreach (var tradingElement in firstData.GetProperty("trading").EnumerateArray())
            tradeHourRecord.HoursRecords.Add(new TradeHourRecord.HoursRecordData
            {
                Day = (DayOfWeek)tradingElement.GetProperty("day").GetInt32(),
                From = ParseFromDatTradeHour(tradingElement
                    .GetProperty("fromT").GetInt64()),
                To = ParseToDatTradeHour(tradingElement.GetProperty("toT")
                    .GetInt64())
            });
        return tradeHourRecord;
    }


    public Position AdaptOpenTradeResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);
        CheckApiStatus(doc);
        var returnDataElement = ReturnData(doc).Value;

        return MapPositionTrasaction(returnDataElement);
    }

    public Position AdaptUpdateTradeResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnDataElement = ReturnData(doc).Value;

        return MapPositionTrasaction(returnDataElement);
    }

    public Position AdaptCloseTradeResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnDataElement = ReturnData(doc).Value;
        return MapPositionTrasaction(returnDataElement);
    }

    public Tick AdaptTickRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        return MapTick(data.Value);
    }

    public Position? AdaptTradeRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);


        var data = ReturnDataStreaming(doc);
        var recordElement = data.Value;
        var customComment = recordElement.GetProperty("customComment").GetString();

        if (!string.IsNullOrEmpty(customComment))
        {
            var position = new Position();

            var order = recordElement.GetProperty("order").GetInt64();
            var order2 = recordElement.GetProperty("order2").GetInt64();
            var positionId = recordElement.GetProperty("position").GetInt64();


            position.Order = $"{order}|{order2}|{positionId}";

            position.Symbol = recordElement.GetProperty("symbol").GetString();
            position.TypePosition =
                FromXtbToRobotAssembler.GetTypeOperation(recordElement.GetProperty("cmd").GetInt64());

            var type = recordElement.GetProperty("type").GetInt32();

            if (recordElement.GetProperty("closed").GetBoolean())
            {
                position.StatusPosition = StatusPosition.Close;
            }
            else
            {
                if (type == 1)
                    position.StatusPosition = StatusPosition.Pending;
                else if (type == 0) position.StatusPosition = StatusPosition.Open;
            }

            position.Profit = recordElement.TryGetProperty("profit", out var profit) &&
                              profit.ValueKind != JsonValueKind.Null
                ? profit.GetDecimal()
                : 0;
            position.OpenPrice = recordElement.GetProperty("open_price").GetDecimal();
            position.DateOpen =
                TimeZoneConverter.ConvertMillisecondsToUtc(recordElement.GetProperty("open_time").GetInt64());
            position.ClosePrice = recordElement.GetProperty("close_price").GetDecimal();
            position.DateClose = recordElement.TryGetProperty("close_time", out var closeDate) &&
                                 closeDate.ValueKind != JsonValueKind.Null
                ? TimeZoneConverter.ConvertMillisecondsToUtc(closeDate.GetInt64())
                : new DateTime();

            position.ReasonClosed =
                FromXtbToRobotAssembler.ComputeCommentReasonClosed(recordElement.GetProperty("comment").GetString());
            position.StopLoss = recordElement.GetProperty("sl").GetDecimal();
            position.TakeProfit = recordElement.GetProperty("tp").GetDecimal();
            position.Volume = recordElement.GetProperty("volume").GetDouble();


            var dataSplit = customComment.Split("|");

            position.SetStrategyId(dataSplit[0]).SetId(dataSplit[1]);

            return position;
        }

        return null;
    }

    public AccountBalance? AdaptBalanceRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        return MapAccountBalanceStreaming(data.Value);
    }

    public Position AdaptTradeStatusRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        var order = data.Value.GetProperty("order").GetInt64();

        return new Position
        {
            Order = $"{order}|{order}|{order}",
            StatusPosition =
                FromXtbToRobotAssembler.ToTradeStatusFromTradeStatusStreaming(data.Value
                    .GetProperty("requestStatus").GetInt64())
        };
    }

    public Position AdaptProfitRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        var order = data.Value.GetProperty("order").GetInt64();
        var order2 = data.Value.GetProperty("order2").GetInt64();
        var positionId = data.Value.GetProperty("position").GetInt64();
        return new Position
        {
            Order = $"{order}|{order2}|{positionId}",
            Profit = data.Value.GetProperty("profit").GetDecimal()
        };
    }

    public News AdaptNewsRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        return MapNews(data.Value);
    }

    public Candle AdaptCandleRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        throw new NotImplementedException();
    }

    private TimeSpan ParseFromDatTradeHour(long time)
    {
        if (time is 0) return TimeSpan.FromMilliseconds(0);

        return TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtcOffset(time);
    }

    private TimeSpan ParseToDatTradeHour(long time)
    {
        var dateRefLimitDay = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).TimeOfDay.TotalMilliseconds;
        if (time is 86400000) return TimeSpan.FromMilliseconds(dateRefLimitDay);

        return TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtcOffset(time);
    }

    public LoginResponse AdaptLoginResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);

        var root = doc.RootElement;

        var streamSessionId = root.GetProperty("streamSessionId").GetString();

        return new LoginResponse()
        {
            StreamingSessionId = streamSessionId
        };
    }

    private Position? MapPosition(JsonElement recordElement)
    {
        var position = new Position();
        var order = recordElement.GetProperty("order").GetInt64();
        var order2 = recordElement.GetProperty("order2").GetInt64();
        var positionId = recordElement.GetProperty("position").GetInt64();

        position.Order = $"{order}|{order2}|{positionId}";

        var positionComment = recordElement.GetProperty("customComment").GetString().Split('|');

        position.StrategyId = positionComment[0];
        position.Id = positionComment[1];

        position.Symbol = recordElement.GetProperty("symbol").GetString();
        position.TypePosition = FromXtbToRobotAssembler.GetTypeOperation(recordElement.GetProperty("cmd").GetInt64());
        position.StatusPosition =
            recordElement.TryGetProperty("type", out var type) && type.ValueKind != JsonValueKind.Null
                ? FromXtbToRobotAssembler.ToTradeStatusFromTradeStreaming(type.GetInt64())
                : StatusPosition.Close;
        position.Profit = recordElement.TryGetProperty("profit", out var profit) &&
                          profit.ValueKind != JsonValueKind.Null
            ? profit.GetDecimal()
            : 0;
        position.OpenPrice = recordElement.GetProperty("open_price").GetDecimal();
        position.DateOpen =
            TimeZoneConverter.ConvertMillisecondsToUtc(recordElement.GetProperty("open_time").GetInt64());
        position.ClosePrice = recordElement.GetProperty("close_price").GetDecimal();
        position.DateClose = recordElement.TryGetProperty("close_time", out var closeDate) &&
                             closeDate.ValueKind != JsonValueKind.Null
            ? TimeZoneConverter.ConvertMillisecondsToUtc(closeDate.GetInt64())
            : new DateTime();

        var comment = recordElement.GetProperty("comment").GetString();

        if (!string.IsNullOrEmpty(comment))
            position.ReasonClosed =
                FromXtbToRobotAssembler.ComputeCommentReasonClosed(comment);

        position.StopLoss = recordElement.GetProperty("sl").GetDecimal();
        position.TakeProfit = recordElement.GetProperty("tp").GetDecimal();
        position.Volume = recordElement.GetProperty("volume").GetDouble();

        return position;
    }

    private Position MapPositionTrasaction(JsonElement jsonElement)
    {
        var position = new Position();
        var order = jsonElement.GetProperty("order").GetInt64();
        var order2 = jsonElement.GetProperty("order").GetInt64();
        var positionId = jsonElement.GetProperty("order").GetInt64();

        position.Order = $"{order}|{order2}|{positionId}";
        return position;
    }

    private void CheckApiStatus(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (root.TryGetProperty("status", out var statusProperty) && !statusProperty.GetBoolean())
        {
            var errorCode = "";
            var errorDescr = "";

            if (root.TryGetProperty("errorCode", out var errorCodeProperty)) errorCode = errorCodeProperty.GetString();

            if (root.TryGetProperty("errorDescr", out var errorDescrProperty))
                errorDescr = errorDescrProperty.GetString();

            if (errorDescr == null && !string.IsNullOrEmpty(errorCode))
                errorDescr = ERR_CODE.getErrorDescription(errorCode);

            throw new ApiException(errorCode, errorDescr);
        }
    }

    private Tick MapTick(JsonElement jsonElement)
    {
        decimal? ask = jsonElement.TryGetProperty("ask", out var askProp) ? askProp.GetDecimal() : null;
        decimal? bid = jsonElement.TryGetProperty("bid", out var bidProp) ? bidProp.GetDecimal() : null;
        var symbol = jsonElement.TryGetProperty("symbol", out var symbolProp) ? symbolProp.GetString() : null;
        decimal? askVolume = jsonElement.TryGetProperty("askVolume", out var askVolumeProp)
            ? askVolumeProp.GetDecimal()
            : null;
        decimal? bidVolume = jsonElement.TryGetProperty("bidVolume", out var bidVolumeProp)
            ? bidVolumeProp.GetDecimal()
            : null;
        var date = jsonElement.TryGetProperty("timestamp", out var timestampProp)
            ? TimeZoneConverter.ConvertMillisecondsToUtc(timestampProp.GetInt64())
            : new DateTime();

        var tick = new Tick
        {
            Ask = ask,
            Bid = bid,
            Symbol = symbol,
            AskVolume = askVolume,
            BidVolume = bidVolume,
            Date = date
        };

        return tick;
    }


    private JsonElement? ReturnData(JsonDocument? doc)
    {
        var root = doc.RootElement;

        if (root.TryGetProperty("returnData", out var returnDataProperty)) return returnDataProperty;

        return null;
    }

    private JsonElement? ReturnDataStreaming(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (root.TryGetProperty("data", out var returnDataProperty)) return returnDataProperty;

        return null;
    }


    private AccountBalance? MapAccountBalance(JsonElement element)
    {
        var accountBalance = new AccountBalance();


        accountBalance.MarginLevel = element.GetProperty("margin_level").GetDouble();
        accountBalance.MarginFree = element.GetProperty("margin_free").GetDouble();
        accountBalance.Margin = element.GetProperty("margin").GetDouble();
        accountBalance.Equity = element.GetProperty("equity").GetDouble();
        accountBalance.Credit = element.GetProperty("credit").GetDouble();
        accountBalance.Balance = element.GetProperty("balance").GetDouble();


        return accountBalance;
    }

    private AccountBalance? MapAccountBalanceStreaming(JsonElement element)
    {
        var accountBalance = new AccountBalance();


        accountBalance.MarginLevel = element.GetProperty("marginLevel").GetDouble();
        accountBalance.MarginFree = element.GetProperty("marginFree").GetDouble();
        accountBalance.Margin = element.GetProperty("margin").GetDouble();
        accountBalance.Equity = element.GetProperty("equity").GetDouble();
        accountBalance.Credit = element.GetProperty("credit").GetDouble();
        accountBalance.Balance = element.GetProperty("balance").GetDouble();


        return accountBalance;
    }

    private News MapNews(JsonElement jsonElement)
    {
        return new News
        {
            Body = jsonElement.GetProperty("body").ToString(),
            Time = TimeZoneConverter.ConvertMillisecondsToUtc(jsonElement.GetProperty("time").GetInt64()),
            Title = jsonElement.GetProperty("title").ToString()
        };
    }


    private Candle MapCandle(JsonElement element, int decimals)
    {
        var open = element.GetProperty("open").GetDecimal() / (decimal)Math.Pow(10, decimals);
        var close = element.GetProperty("close").GetDecimal();
        var high = element.GetProperty("high").GetDecimal();
        var low = element.GetProperty("low").GetDecimal();
        close = open + close / (decimal)Math.Pow(10, decimals);
        high = open + high / (decimal)Math.Pow(10, decimals);
        low = open + low / (decimal)Math.Pow(10, decimals);

        return new Candle
        {
            Close = close,
            Date = TimeZoneConverter.ConvertMillisecondsToUtc(element.GetProperty("ctm").GetInt64()),
            High = high,
            Low = low,
            Open = open,
            Volume = element.GetProperty("vol").GetDecimal()
        };
    }
}