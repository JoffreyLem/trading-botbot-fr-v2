using RobotAppLibraryV2.ApiConnector.Modeles;

namespace RobotAppLibraryV2.Api.Xtb;

public static class XtbServer
{
    public static Server DEMO_TCP => new("xapi.xtb.com", 5124, 5125, "DEMO SSL");

    public static Server REAL_TCP => new("xapi.xtb.com", 5112, 5113, "REAL SSL");


    public static Server DEMO_WSS => new("wss://ws.xtb.com/demo", "DEMO WSS");

    public static Server DEMO_WSS_STREAMING => new("wss://ws.xtb.com/demoStream", "DEMO WSS STREAMING");

    public static Server REAL_WSS => new("wss://ws.xtb.com/real", "REAL WSS");

    public static Server REAL_WSS_STREAMING => new("wss://ws.xtb.com/realStream", "REAL WSS STREAMING");
}