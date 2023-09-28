namespace RobotAppLibraryV2.Modeles;

public sealed class SymbolInfo
{
    public Category Category { get; set; }
    public long? ContractSize { get; set; }

    /// <summary>
    ///     Base Currency
    /// </summary>
    public string? Currency1 { get; set; }

    /// <summary>
    ///     Currency profit
    /// </summary>
    public string? Currency2 { get; set; }

    public double? LotMax { get; set; }
    public double? LotMin { get; set; }

    public long? Precision { get; set; }
    public string? Symbol { get; set; }
    public double TickSize { get; set; }

    /// <summary>
    ///     For money management
    /// </summary>
    public double TickSize2 { get; set; }

    public double TickValue { get; set; }

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

    public SymbolInfo WithCurrency1(string currency1)
    {
        Currency1 = currency1;
        return this;
    }

    public SymbolInfo WithCurrency2(string currency2)
    {
        Currency2 = currency2;
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

    public SymbolInfo WithTickValue(double tickValue)
    {
        TickValue = tickValue;
        return this;
    }

    public SymbolInfo WithTickSize2(double tickValue)
    {
        TickSize2 = tickValue;
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