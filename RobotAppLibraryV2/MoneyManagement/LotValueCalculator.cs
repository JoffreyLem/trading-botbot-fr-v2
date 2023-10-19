using System.Diagnostics.CodeAnalysis;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.MoneyManagement;

// TODO : TU Direct ici
public class LotValueCalculator : IDisposable
{
    private const int StandardLotSize = 100000;

    private readonly IApiHandler _apiHandler;

    private readonly ILogger? _logger;

    private string? _secondarySymbolAccount;

    private Tick _tickPriceMain = new();

    public LotValueCalculator(IApiHandler apiHandler, ILogger? logger, string symbol, bool subscribePrice = true)
    {
        _apiHandler = apiHandler;
        _logger = logger;
        Init(subscribePrice, symbol);
    }

    public string BaseSymbolAccount { get; set; } = "EUR";
    public SymbolInfo SymbolInfo { get; private set; } = null!;
    public double LotValueStandard { get; private set; }

    public double MiniLot => LotValueStandard / 10;
    public double MicroLot => LotValueStandard / 100;
    public double NanoLot => LotValueStandard / 1000;

    public Tick? _tickPriceSecondary { get; private set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Init(bool subscribePrice, string symbol)
    {
        SymbolInfo = _apiHandler.GetSymbolInformationAsync(symbol).Result;
        _tickPriceMain = _apiHandler
            .GetTickPriceAsync(SymbolInfo.Symbol ?? throw new InvalidOperationException("Symbol is not defined"))
            .Result;

        if (SymbolInfo.Category == Category.Forex && !SymbolInfo.Symbol.Contains(BaseSymbolAccount))
            SubscribeSecondaryPrice(subscribePrice);
        else if (SymbolInfo.Category == Category.Indices && SymbolInfo.Currency2 != BaseSymbolAccount)
            SubscribeSecondaryPrice(subscribePrice);

        SymbolSwitch();

        if (subscribePrice) _apiHandler.TickEvent += ApiHandlerOnTickEvent;
    }

    private void SubscribeSecondaryPrice(bool subscribePrice)
    {
        var symbol1 = BaseSymbolAccount;
        var symbol2 = SymbolInfo.Currency2;
        _secondarySymbolAccount = GetMachingSymbolWithCurrency(symbol1, symbol2);
        _tickPriceSecondary = _apiHandler.GetTickPriceAsync(_secondarySymbolAccount).Result;
        if (subscribePrice) _apiHandler.SubscribePrice(_secondarySymbolAccount);
    }

    private void SymbolSwitch()
    {
        switch (SymbolInfo.Category)
        {
            case Category.Forex:
                HandleForex();
                break;
            case Category.Indices:
                HandleIndices();
                break;
            default:
                throw new ArgumentException($"Symbol type {SymbolInfo.Category} non gerer");
        }
    }

    private void ApiHandlerOnTickEvent(object? sender, Tick e)
    {
        if (e.Symbol == SymbolInfo.Symbol)
        {
            _tickPriceMain = e;
            SymbolSwitch();
        }

        if (e.Symbol == _secondarySymbolAccount)
        {
            _tickPriceSecondary = e;
            SymbolSwitch();
        }
    }

    private void HandleForex()
    {
        var pipValue = SymbolInfo.TickSize2 * StandardLotSize;

        if (SymbolInfo.Currency2 == BaseSymbolAccount)
        {
            LotValueStandard = pipValue;
        }
        else
        {
            if (SymbolInfo.Currency1 == BaseSymbolAccount)
                LotValueStandard = pipValue / (double)_tickPriceMain.Bid.GetValueOrDefault();
            else
                LotValueStandard = pipValue / (double)_tickPriceSecondary.GetValueOrDefault().Bid.GetValueOrDefault();
        }

        _logger?.Information("New lot value forex : {Lot}", LotValueStandard);
    }


    private void HandleIndices()
    {
        if (SymbolInfo.Currency2 == BaseSymbolAccount)
        {
            LotValueStandard = SymbolInfo.ContractSize.GetValueOrDefault();
        }
        else
        {
            LotValueStandard = (double)(SymbolInfo.ContractSize.GetValueOrDefault() /
                                        _tickPriceSecondary.GetValueOrDefault().Bid.GetValueOrDefault());
            _logger?.Information("New lot value indices : {Lot}", LotValueStandard);
        }
    }


    private string GetMachingSymbolWithCurrency(string symbol1, string symbol2)
    {
        try
        {
            var allSymbol = _apiHandler.GetAllSymbolsAsync().Result;
            var selected =
                allSymbol.ToList().Find(x => x.StartsWith(symbol1) && x.EndsWith(symbol2));
            return selected ?? throw new Exception($"No matchin symbol for {symbol1} : {symbol2}");
        }
        catch (Exception e)
        {
            throw new MoneyManagementException("", e);
        }
    }


    [ExcludeFromCodeCoverage]
    protected virtual void Dispose(bool disposing)
    {
        _apiHandler.TickEvent += null;
        if (_secondarySymbolAccount is not null && _apiHandler.IsConnected()) _apiHandler.UnsubscribePrice(_secondarySymbolAccount);
    }
}