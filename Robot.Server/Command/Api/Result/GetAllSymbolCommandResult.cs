using RobotAppLibraryV2.Modeles;

namespace Robot.Server.Command.Api.Result;

public class GetAllSymbolCommandResultCommand : ServiceCommandResponseBase
{
    public List<SymbolInfo> SymbolInfos { get; set; }
}