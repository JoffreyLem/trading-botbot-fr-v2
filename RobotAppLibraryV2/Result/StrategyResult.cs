using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Result;

public class StrategyResult
{
    private readonly List<Position> _positionInternal = new();

    public StrategyResult()
    {
    }

    public StrategyResult(List<Position>? positions)
    {
        if (positions != null)
        {
            _positionInternal.AddRange(positions);
            CalculateResults();
        }
    }

    public IReadOnlyList<Position> Positions => _positionInternal.AsReadOnly();

    public Modeles.Result Results { get; set; } = new();

    public void UpdateGlobalData(Position position)
    {
        _positionInternal.Add(position);
        Results = CalculateResults();
    }

    public void UpdateGlobalData(List<Position>? positions)
    {
        if (positions?.Count > 0)
        {
            _positionInternal.AddRange(positions);
            Results = CalculateResults();
        }
    }

    public Modeles.Result CalculateResults()
    {
        try
        {
            if (!_positionInternal.Any()) return Results;

            if (_positionInternal.Exists(x => x.Profit > 0))
            {
                Results.GainMax = _positionInternal.Max(x => x.Profit);
                Results.ProfitPositif = _positionInternal.Where(x => x.Profit > 0).Sum(x => x.Profit);
                Results.TotalPositionPositive = _positionInternal.Count(x => x.Profit > 0);
                Results.MoyennePositive = _positionInternal.Where(x => x.Profit > 0).Average(x => x.Profit);
            }

            if (_positionInternal.Exists(x => x.Profit < 0))
            {
                Results.PerteMax = _positionInternal.Min(x => x.Profit);
                Results.ProfitNegatif = _positionInternal.Where(x => x.Profit < 0).Sum(x => x.Profit);
                Results.TotalPositionNegative = _positionInternal.Count(x => x.Profit < 0);
                Results.MoyenneNegative = _positionInternal.Where(x => x.Profit < 0).Average(x => x.Profit);
            }

            Results.Profit = _positionInternal.Sum(x => x.Profit);
            Results.TotalPositions = _positionInternal.Count;
            Results.MoyenneProfit = _positionInternal.Average(x => x.Profit);
            if (Results.MoyenneNegative != 0)
                Results.RatioMoyennePositifNegatif =
                    Results.MoyennePositive / Results.MoyenneNegative;


            if (Results.ProfitNegatif != 0)
                Results.ProfitFactor = Math.Abs(Results.ProfitPositif / Results.ProfitNegatif);

            if (Results.TotalPositions != 0)
                Results.TauxReussite =
                    Results.TotalPositionPositive / Results.TotalPositions * 100;

            CalculateDrawdowns();
            return Results;
        }
        catch (Exception e)
        {
            throw new ResultException("Error on calculating result", e);
        }
    }

    private void CalculateDrawdowns()
    {
        var peakValue = _positionInternal[0].Profit;
        decimal drawdownMax = 0;
        decimal drawdown = 0;

        foreach (var profit in _positionInternal.OrderBy(x => x.DateClose).Select(x => x.Profit))
        {
            peakValue = Math.Max(peakValue, profit);
            Results.Drawndown = Math.Abs(peakValue - profit);

            // TODO : Partie à vérifier
            if (drawdown > drawdownMax) Results.DrawndownMax = drawdown;
        }
    }
}