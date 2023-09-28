namespace RobotAppLibraryV2.ApiHandler.Xtb;

public interface IConnector
{
    public void Disconnect(bool silent = false);

    public bool Connected();
}