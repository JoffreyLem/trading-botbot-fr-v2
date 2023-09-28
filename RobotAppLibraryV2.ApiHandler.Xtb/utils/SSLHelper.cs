using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace RobotAppLibraryV2.ApiHandler.Xtb.utils;

internal class SSLHelper
{
    /// <summary>
    ///     Validator that trusts all SSL certificates (all traffic is cyphered).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="cert"></param>
    /// <param name="chain"></param>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static bool TrustAllCertificatesCallback(object sender, X509Certificate cert, X509Chain chain,
        SslPolicyErrors errors)
    {
        return true;
    }
}