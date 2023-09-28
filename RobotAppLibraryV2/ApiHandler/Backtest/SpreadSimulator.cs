namespace RobotAppLibraryV2.ApiHandler.Backtest;

public class SpreadSimulator
{
    private readonly decimal _maxSpread;
    private readonly decimal _minSpread;
    private readonly Random _random;

    public SpreadSimulator(decimal minSpread, decimal maxSpread)
    {
        _random = new Random();
        _minSpread = minSpread;
        _maxSpread = maxSpread;
    }

    public decimal GenerateSpread()
    {
        var range = _maxSpread - _minSpread;
        var spread = _minSpread + (decimal)_random.NextDouble() * range;

        return Math.Round(spread, 1);
    }
}