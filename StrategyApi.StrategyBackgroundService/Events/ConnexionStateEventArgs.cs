using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Events;

public class ConnexionStateEventArgs : EventArgs
{
    public ReferentEnum Referent { get; set; }

    public ConnexionStateEnum ConnexionState { get; set; }
}