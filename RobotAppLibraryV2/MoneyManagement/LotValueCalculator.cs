using System.Diagnostics.CodeAnalysis;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.MoneyManagement;

public class LotValueCalculator : ILotValueCalculator, IDisposable
{
    private const int STANDARD_LOT_SIZE = 100000;

    private readonly IApiHandler _apiHandler;

    private readonly ILogger? _logger;

    private string? _secondarySymbolAccount;

    private Tick _tickPriceMain = new();

    public LotValueCalculator(IApiHandler apiHandler, ILogger? logger, string symbol)
    {
        _apiHandler = apiHandler;
        _logger = logger;
        Init(symbol);
    }

    private string BaseSymbolAccount { get; } = "EUR";
    private SymbolInfo SymbolInfo { get; set; } = null!;
    public double PipValueStandard { get; private set; }
    public double PipValueMiniLot => PipValueStandard / 10;
    public double PipValueMicroLot => PipValueStandard / 100;
    public double PipValueNanoLot => PipValueStandard / 1000;
    public double MarginPerLot { get; private set; }

    public Tick? TickPriceSecondary { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Init(string symbol)
    {
        SymbolInfo = _apiHandler.GetSymbolInformationAsync(symbol).Result;
        _tickPriceMain = _apiHandler
            .GetTickPriceAsync(SymbolInfo.Symbol ?? throw new InvalidOperationException("Symbol is not defined"))
            .Result;

        if (SymbolInfo.Category == Category.Forex && !SymbolInfo.Symbol.Contains(BaseSymbolAccount))
            SubscribeSecondaryPrice();
        else if (SymbolInfo.Category != Category.Forex && SymbolInfo.CurrencyProfit != BaseSymbolAccount)
            SubscribeSecondaryPrice();

        SymbolSwitch();
        _apiHandler.TickEvent += ApiHandlerOnTickEvent;
    }

    private void SubscribeSecondaryPrice()
    {
        var symbol1 = BaseSymbolAccount;
        var symbol2 = SymbolInfo.CurrencyProfit;
        _secondarySymbolAccount = GetMachingSymbolWithCurrency(symbol1, symbol2);
        TickPriceSecondary = _apiHandler.GetTickPriceAsync(_secondarySymbolAccount).Result;
        _apiHandler.SubscribePrice(_secondarySymbolAccount);
    }

    private void SymbolSwitch()
    {
        switch (SymbolInfo.Category)
        {
            case Category.Forex:
                HandleForex();
                break;
            default:
                HandleOtherSymbols();
                break;
        }
    }

    private void ApiHandlerOnTickEvent(object? sender, Tick e)
    {
        try
        {
            if (e.Symbol == SymbolInfo.Symbol)
            {
                _tickPriceMain = e;
                SymbolSwitch();
            }

            if (e.Symbol == _secondarySymbolAccount)
            {
                TickPriceSecondary = e;
                SymbolSwitch();
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Error when updating lot value");
        }
    }

    private void HandleForex()
    {
        var tickSize = SymbolInfo.Symbol.Contains("JPY") ? 0.01m : 0.0001m;
        var pipValue = tickSize * STANDARD_LOT_SIZE;

        if (SymbolInfo.CurrencyProfit == BaseSymbolAccount)
        {
            PipValueStandard = (double)pipValue;
        }
        else
        {
            if (SymbolInfo.Currency == BaseSymbolAccount)
                PipValueStandard = (double)(pipValue / _tickPriceMain.Bid.GetValueOrDefault());
            else
                PipValueStandard = (double)(pipValue / TickPriceSecondary.GetValueOrDefault().Bid.GetValueOrDefault());
        }

        _logger?.Debug("New lot value forex : {Lot}", PipValueStandard);

        var leverageRatio = SymbolInfo.Leverage != 0 ? 100 / SymbolInfo.Leverage : 0;

        if (leverageRatio > 0)
            MarginPerLot = SymbolInfo.Leverage * STANDARD_LOT_SIZE / 100;
        else
            MarginPerLot = PipValueStandard * STANDARD_LOT_SIZE;

        _logger?.Debug("Required margin per standard lot: {MarginPerLot}", MarginPerLot);
    }


    private void HandleOtherSymbols()
    {
        if (SymbolInfo.CurrencyProfit == BaseSymbolAccount)
            PipValueStandard = SymbolInfo.ContractSize.GetValueOrDefault();
        else
            PipValueStandard = (double)(SymbolInfo.ContractSize.GetValueOrDefault() /
                                        TickPriceSecondary.GetValueOrDefault().Bid.GetValueOrDefault());

        _logger?.Debug("New lot value indices : {Lot}", PipValueStandard);
        var leverageRatio = 100 / SymbolInfo.Leverage;
        MarginPerLot = PipValueStandard * (double)_tickPriceMain.Bid.GetValueOrDefault() / leverageRatio;
        _logger?.Debug($"Marge requise par lot : {MarginPerLot}");
    }


    private string GetMachingSymbolWithCurrency(string symbol1, string symbol2)
    {
        try
        {
            var allSymbol = _apiHandler.GetAllSymbolsAsync().Result;
            var selected =
                allSymbol.ToList().Find(x => x.Symbol.StartsWith(symbol1) && x.Symbol.EndsWith(symbol2));
            return selected.Symbol ?? throw new Exception($"No matchin symbol for {symbol1} : {symbol2}");
        }
        catch (Exception e)
        {
            throw new MoneyManagementException("", e);
        }
    }


    [ExcludeFromCodeCoverage]
    protected virtual void Dispose(bool disposing)
    {
        _apiHandler.TickEvent -= null;
        if (_secondarySymbolAccount is not null && _apiHandler.IsConnected())
            _apiHandler.UnsubscribePrice(_secondarySymbolAccount);
    }
}