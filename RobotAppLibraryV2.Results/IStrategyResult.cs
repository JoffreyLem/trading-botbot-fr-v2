using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Results;

public interface IStrategyResult : IDisposable
{
    IReadOnlyList<Position> Positions { get; }
    Result Results { get; set; }

    /// <summary>
    ///     In percentage.
    /// </summary>
    double Risque { get; set; }

    int LooseStreak { get; set; }
    double ToleratedDrawnDown { get; set; }
    bool SecureControlPosition { get; set; }
    void UpdateGlobalData(Position position);
    void UpdateGlobalData(List<Position>? positions);
    Result CalculateResults();

    public event EventHandler<EventTreshold>? ResultTresholdEvent;
}