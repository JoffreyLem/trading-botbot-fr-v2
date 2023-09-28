namespace RobotAppLibraryV2.ApiHandler.Xtb.sync;

public class Credentials
{
    [Obsolete("Up from 2.3.3 login is not a long, but string")]
    public Credentials(long login, string password, string appId = "", string appName = "")
    {
        Login = login.ToString();
        Password = password;
        AppId = appId;
        AppName = appName;
    }

    public Credentials(string login, string password)
    {
        Login = login;
        Password = password;
    }

    public Credentials(string login, string password, string appId, string appName)
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