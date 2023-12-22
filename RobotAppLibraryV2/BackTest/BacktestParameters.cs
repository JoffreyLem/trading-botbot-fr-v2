using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.BackTest;

public class BacktestParameters
{
    public BacktestParameters(string symbol, Timeframe timeframe,  double balance, decimal minSpread, decimal maxSpread)
    {
        Symbol = symbol;
        Timeframe = timeframe;
        Balance = balance;
  
        SpreadSimulator = new SpreadSimulator(minSpread, maxSpread);
    }

    public string Symbol { get;  set; }
    public Timeframe Timeframe { get;  set; }
    public double Balance { get;  set; }

    public SpreadSimulator SpreadSimulator;

}