using RobotAppLibraryV2.ApiHandler.Interfaces;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Result;

public class StrategyResult : IStrategyResult
{
    private readonly IApiHandler _apiHandler;
    private readonly List<Position> _positionInternal = new();
    private readonly string positionReference;

    private AccountBalance accountBalance = new();

    public StrategyResult(IApiHandler apiHandler, string positionReference)
    {
        _apiHandler = apiHandler;
        this.positionReference = positionReference;
        accountBalance = _apiHandler.GetBalanceAsync().Result;
        var listPositions = _apiHandler.GetAllPositionsByCommentAsync(positionReference).Result;
        UpdateGlobalData(listPositions);
        _apiHandler.PositionClosedEvent += ApiHandlerOnPositionClosedEvent;
        _apiHandler.NewBalanceEvent += (sender, balance) => accountBalance = balance;
    }


    public double Risque { get; set; } = 2;

    public int LooseStreak { get; set; } = 10;
    public double ToleratedDrawnDown { get; set; } = 10;
    public bool SecureControlPosition { get; set; }

    public event EventHandler<EventTreshold>? ResultTresholdEvent;

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

    public void Dispose()
    {
    }

    private void ApiHandlerOnPositionClosedEvent(object? sender, Position e)
    {
        if (e.StrategyId == positionReference)
        {
            UpdateGlobalData(e);
            if (SecureControlPosition) TresholdCheck();
        }
    }

    private void TresholdCheck()
    {
        CheckDrawnDownTreshold();
        CheckLooseStreakTreshold();
        CheckProfitFactorTreshold();
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

    private void CheckDrawnDownTreshold()
    {
        var drawndown = Results.Drawndown;
        var drawDownTheorique = accountBalance.Balance * (ToleratedDrawnDown / 100);

        if (drawndown > 0 && drawndown >= (decimal)drawDownTheorique)
            ResultTresholdEvent?.Invoke(this, EventTreshold.Drowdown);
    }

    private void CheckLooseStreakTreshold()
    {
        var selected = Positions.TakeLast(LooseStreak).ToList();

        if (selected.Count == LooseStreak && selected.TrueForAll(x => x.Profit < 0))
            ResultTresholdEvent?.Invoke(this, EventTreshold.LooseStreak);
    }

    private void CheckProfitFactorTreshold()
    {
        var profitfactor = Results.ProfitFactor;
        if (profitfactor is > 0 and <= 1) ResultTresholdEvent?.Invoke(this, EventTreshold.Profitfactor);
    }
}