namespace RobotAppLibraryV2.Modeles;

public sealed class SymbolInfo
{
    public Category Category { get; set; }
    public long? ContractSize { get; set; }

    /// <summary>
    ///     Base Currency
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    ///     Currency profit
    /// </summary>
    public string? CurrencyProfit { get; set; }

    public bool CurrencyPair { get; set; }

    public double? LotMax { get; set; }
    public double? LotMin { get; set; }

    public long? Precision { get; set; }
    public string? Symbol { get; set; }
    public double TickSize { get; set; }


    /// <summary>
    ///     In percentage
    /// </summary>
    public double Leverage { get; set; }


    public SymbolInfo WithCategory(Category category)
    {
        Category = category;
        return this;
    }

    public SymbolInfo WithContractSize(long? contractSize)
    {
        ContractSize = contractSize;
        return this;
    }

    public SymbolInfo WithCurrency(string currency1)
    {
        Currency = currency1;
        return this;
    }

    public SymbolInfo WithCurrencyProfit(string currency2)
    {
        CurrencyProfit = currency2;
        return this;
    }

    public SymbolInfo WithCurrencyPair(bool currencyPair)
    {
        CurrencyPair = currencyPair;
        return this;
    }

    public SymbolInfo WithLotMax(double? lotMax)
    {
        LotMax = lotMax;
        return this;
    }

    public SymbolInfo WithLotMin(double? lotMin)
    {
        LotMin = lotMin;
        return this;
    }


    public SymbolInfo WithPrecision(long? precision)
    {
        Precision = precision;
        return this;
    }

    public SymbolInfo WithSymbol(string symbol)
    {
        Symbol = symbol;
        return this;
    }

    public SymbolInfo WithTickSize(double tickSize)
    {
        TickSize = tickSize;
        return this;
    }


    public SymbolInfo WithLeverage(double leverage)
    {
        Leverage = leverage;
        return this;
    }
}

public enum Category
{
    Forex,
    Indices,
    Unknow
}