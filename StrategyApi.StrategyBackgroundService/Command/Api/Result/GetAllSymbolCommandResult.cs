using RobotAppLibraryV2.Modeles;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class GetAllSymbolCommandResultCommand : ServiceCommandResponse
{
    public List<SymbolInfo> SymbolInfos { get; set; }
}