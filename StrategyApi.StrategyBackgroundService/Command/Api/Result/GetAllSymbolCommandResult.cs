using RobotAppLibraryV2.Modeles;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class GetAllSymbolCommandResultCommand : ServiceCommandResponseBase
{
    public List<SymbolInfo> SymbolInfos { get; set; }
}