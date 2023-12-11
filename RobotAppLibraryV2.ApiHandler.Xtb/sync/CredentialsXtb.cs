namespace RobotAppLibraryV2.ApiHandler.Xtb.sync;

public class CredentialsXtb
{
    [Obsolete("Up from 2.3.3 login is not a long, but string")]
    public CredentialsXtb(long login, string password, string appId = "", string appName = "")
    {
        Login = login.ToString();
        Password = password;
        AppId = appId;
        AppName = appName;
    }

    public CredentialsXtb(string login, string password)
    {
        Login = login;
        Password = password;
    }

    public CredentialsXtb(string login, string password, string appId, string appName)
    {
        Login = login;
        Password = password;
        AppId = appId;
        AppName = appName;
    }

    public string Login { get; }

    public string Password { get; }

    public string AppId { get; set; }

    public string AppName { get; set; }

    public override string ToString()
    {
        return "Credentials [login=" + Login + ", password=" + Password + "]";
    }
}