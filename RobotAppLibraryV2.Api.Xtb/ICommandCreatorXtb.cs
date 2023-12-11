using RobotAppLibraryV2.ApiConnector.Interfaces;

namespace RobotAppLibraryV2.Api.Xtb;

public interface ICommandCreatorXtb : ICommandCreator
{
    public string? StreamingSessionId { get; set; }
}