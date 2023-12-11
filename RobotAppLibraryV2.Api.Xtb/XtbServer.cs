using RobotAppLibraryV2.ApiConnector.Modeles;

namespace RobotAppLibraryV2.Api.Xtb;

public static class XtbServer
{
    /// <summary>
    ///     Demo port set.
    /// </summary>
    private static readonly PortSet DEMO_PORTS = new(5124, 5125);

    /// <summary>
    ///     Real port set.
    /// </summary>
    private static readonly PortSet REAL_PORTS = new(5112, 5113);

    private static List<Server> demoServers;
    private static List<Server> realServers;
    private static List<ApiAddress> addresses;

    /// <summary>
    ///     List of all available addresses.
    /// </summary>
    private static List<ApiAddress> ADDRESSES
    {
        get
        {
            if (addresses == null)
            {
                addresses = new List<ApiAddress>();

                addresses.Add(new ApiAddress("xapi.xtb.com", "xAPI A"));
                addresses.Add(new ApiAddress("xapi.xtb.com", "xAPI B"));
            }

            return addresses;
        }
    }

    /// <summary>
    ///     xAPI Demo Server.
    /// </summary>
    public static Server DEMO => DEMO_SERVERS[0];

    /// <summary>
    ///     xAPI Real Server.
    /// </summary>
    public static Server REAL => REAL_SERVERS[0];

    /// <summary>
    ///     List of all demo servers.
    /// </summary>
    public static List<Server> DEMO_SERVERS
    {
        get
        {
            if (demoServers == null)
            {
                demoServers = new List<Server>();

                foreach (var address in ADDRESSES)
                    demoServers.Add(new Server(address.Address, DEMO_PORTS.MainPort, DEMO_PORTS.StreamingPort,
                        address.Name + " DEMO SSL"));

                demoServers.Shuffle();
            }

            return demoServers;
        }
    }

    /// <summary>
    ///     List of all real servers.
    /// </summary>
    public static List<Server> REAL_SERVERS
    {
        get
        {
            if (realServers == null)
            {
                realServers = new List<Server>();

                foreach (var address in ADDRESSES)
                    realServers.Add(new Server(address.Address, REAL_PORTS.MainPort, REAL_PORTS.StreamingPort,
                        address.Name + " REAL SSL"));

                realServers.Shuffle();
            }

            return realServers;
        }
    }


    /// <summary>
    ///     Extends List with shuffle method.
    /// </summary>
    /// <typeparam name="T">List type</typeparam>
    /// <param name="list">List to shuffle</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    ///     Represents a set of ports (main and streaming) for a single connection.
    /// </summary>
    public class PortSet
    {
        public PortSet(int mainPort, int streamingPort)
        {
            MainPort = mainPort;
            StreamingPort = streamingPort;
        }

        public int MainPort { get; }

        public int StreamingPort { get; }
    }

    /// <summary>
    ///     Represents a single xAPI address.
    /// </summary>
    public class ApiAddress
    {
        public ApiAddress(string address, string name)
        {
            Address = address;
            Name = name;
        }

        public string Address { get; }

        public string Name { get; }
    }
}