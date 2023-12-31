using RobotAppLibraryV2.ApiHandler;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.PositionHandler;

public class PositionHandler : IPositionHandler
{
    private readonly IApiHandler _apiHandler;
    private readonly ILogger _logger;
    private readonly string _symbol;
    private int _precision;
    private SymbolInfo _symbolInfo = new();

    public PositionHandler(ILogger logger, IApiHandler apiHandler, string symbol, string positionId)
    {
        _logger = logger.ForContext<PositionHandler>();
        _apiHandler = apiHandler;
        _symbol = symbol;
        PositionId = positionId;
        Init();
    }

    public string PositionId { get; init; }

    public int DefaultSl { get; set; } = 20;
    public int DefaultTp { get; set; } = 20;
    public Position? PositionOpened { get; private set; }
    public Position? PositionPending { get; private set; }
    public Tick LastPrice { get; private set; } = new();
    public bool PositionInProgress => PositionOpened is not null || PositionPending is not null;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;

    public async Task OpenPositionAsync(string symbol, TypeOperation typePosition, double volume,
        decimal sl = 0,
        decimal tp = 0, long? expiration = 0)
    {
        try
        {
            sl = sl != 0
                ? Math.Round(sl, _precision)
                : CalculateStopLoss(DefaultSl, typePosition);
            tp = tp != 0
                ? Math.Round(tp, _precision)
                : CalculateTakeProfit(DefaultTp, typePosition);
            var priceData = typePosition == TypeOperation.Buy
                ? LastPrice.Ask.GetValueOrDefault()
                : LastPrice.Bid.GetValueOrDefault();
            var positionModele = new Position();
            positionModele
                .SetSymbol(symbol)
                .SetId(Guid.NewGuid().ToString())
                .SetTypePosition(typePosition)
                .SetSpread(LastPrice.Spread)
                .SetOpenPrice(priceData)
                .SetStopLoss(sl)
                .SetTakeProfit(tp)
                .SetVolume(volume)
                .SetStrategyId(PositionId);
            PositionPending = positionModele;
            _logger.Information("Send position to handler {@Position}", positionModele);
            await _apiHandler.OpenPositionAsync(positionModele, LastPrice.Bid.GetValueOrDefault());
        }
        catch (Exception e)
        {
            PositionPending = null;
            _logger.Error(e, "Error on open position");
        }
    }


    public async Task UpdatePositionAsync(Position position)
    {
        try
        {
            _logger.Information("Send position {Id} for update", position.Id);

            if (position.StatusPosition is not StatusPosition.Close)
                if (PositionOpened?.StopLoss != position.StopLoss ||
                    PositionOpened?.TakeProfit != position.TakeProfit)
                {
                    position.StopLoss = Math.Round(position.StopLoss, _precision);
                    position.TakeProfit = Math.Round(position.TakeProfit, _precision);
                    var priceData = position.TypePosition == TypeOperation.Buy ? LastPrice.Ask : LastPrice.Bid;
                    await _apiHandler.UpdatePositionAsync(priceData.GetValueOrDefault(), position);
                }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Position {Id} can't be update", position.Id);
        }
    }

    public async Task ClosePositionAsync(Position position)
    {
        try
        {
            if (position.StatusPosition is not StatusPosition.Close)
            {
                _logger.Information("Send position {Id} to handler for close", position.Id);
                var closeprice = position.TypePosition == TypeOperation.Buy ? LastPrice.Ask : LastPrice.Bid;
                position.StatusPosition = StatusPosition.Close;
                await _apiHandler.ClosePositionAsync(closeprice.GetValueOrDefault(), position);
            }
        }
        catch (Exception e)
        {
            position.StatusPosition = StatusPosition.Open;
            _logger.Error(e, "Position {PositionId} close error api", position.Id);
        }
    }


    public decimal CalculateStopLoss(decimal pips, TypeOperation positionType)
    {
        if (_precision > 1) pips *= (decimal)_symbolInfo.TickSize;


        switch (positionType)
        {
            case TypeOperation.Buy:
                return Math.Round(LastPrice.Bid.GetValueOrDefault() - pips, _precision);
            case TypeOperation.Sell:
                return Math.Round(LastPrice.Ask.GetValueOrDefault() + pips, _precision);
            default:
                throw new ArgumentException("Invalid position type");
        }
    }


    public decimal CalculateTakeProfit(decimal pips, TypeOperation positionType)
    {
        if (_precision > 1) pips *= (decimal)_symbolInfo.TickSize;

        switch (positionType)
        {
            case TypeOperation.Buy:
                return Math.Round(LastPrice.Ask.GetValueOrDefault() + pips, _precision);
            case TypeOperation.Sell:
                return Math.Round(LastPrice.Bid.GetValueOrDefault() - pips, _precision);
            default:
                throw new ArgumentException("Invalid position type");
        }
    }

    private void Init()
    {
        _symbolInfo = _apiHandler.GetSymbolInformationAsync(_symbol).Result;
        LastPrice = _apiHandler.GetTickPriceAsync(_symbol).Result;
        _apiHandler.TickEvent += ApiHandlerOnTickEvent;
        _apiHandler.PositionOpenedEvent += ApiHandlerOnPositionOpenedEvent;
        _apiHandler.PositionUpdatedEvent += ApiHandlerOnPositionUpdatedEvent;
        _apiHandler.PositionRejectedEvent += ApiHandlerOnPositionRejectedEvent;
        _apiHandler.PositionClosedEvent += ApiHandlerOnPositionClosedEvent;

        var currentPosition = _apiHandler.GetCurrentTradeAsync(PositionId).Result;

        if (currentPosition is not null)
        {
            _apiHandler.RestoreSession(currentPosition);
            PositionOpened = currentPosition;
        }

        CalculatePrecision();
    }


    private void ApiHandlerOnTickEvent(object? sender, Tick e)
    {
        if (e.Symbol == _symbol) LastPrice = e;
    }

    private void ApiHandlerOnPositionOpenedEvent(object? sender, Position e)
    {
        if (e.PositionStrategyReferenceId == PositionPending?.PositionStrategyReferenceId)
        {
            PositionOpened = e;
            e.StatusPosition = StatusPosition.Open;
            PositionPending = null;
            _logger.Information("Position opened : {EId}", e);
            PositionOpenedEvent?.Invoke(this, e);
        }
    }

    private void ApiHandlerOnPositionRejectedEvent(object? sender, Position e)
    {
        if (e.PositionStrategyReferenceId == PositionPending?.PositionStrategyReferenceId)
        {
            PositionPending = null;
            _logger.Information("Position rejected : {EId}", e.Id);
            e.StatusPosition = StatusPosition.Rejected;
            PositionRejectedEvent?.Invoke(this, e);
        }
    }


    private void ApiHandlerOnPositionUpdatedEvent(object? sender, Position e)
    {
        if (PositionOpened is not null && e.PositionStrategyReferenceId == PositionOpened?.PositionStrategyReferenceId)
        {
            PositionOpened.Profit = e.Profit;
            PositionOpened.StopLoss = e.StopLoss;
            PositionOpened.TakeProfit = e.TakeProfit;
            PositionUpdatedEvent?.Invoke(this, e);
        }
    }


    private void ApiHandlerOnPositionClosedEvent(object? sender, Position e)
    {
        if (PositionOpened is not null && PositionOpened?.PositionStrategyReferenceId == e.PositionStrategyReferenceId)
        {
            _logger.Information("Position Closed : {@EId}", e);
            PositionClosedEvent?.Invoke(this, e);
            PositionOpened = null;
        }
    }

    private void CalculatePrecision()
    {
        _precision = BitConverter.GetBytes(decimal.GetBits((decimal)_symbolInfo.TickSize)[3])[2];
    }
}