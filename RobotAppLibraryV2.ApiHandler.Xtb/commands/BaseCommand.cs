using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.errors;

namespace RobotAppLibraryV2.ApiHandler.Xtb.commands;

using JSONObject = JObject;

public abstract class BaseCommand
{
    protected internal JSONObject arguments;
    protected internal string commandName;
    protected internal bool? prettyPrint;

    public BaseCommand(bool? prettyPrint) : this(new JSONObject(), prettyPrint)
    {
    }

    public BaseCommand(JSONObject arguments, bool? prettyPrint, string customTag = "")
    {
        commandName = CommandName;
        this.arguments = arguments;
        this.prettyPrint = prettyPrint;

        if (customTag == "") customTag = utils.CustomTag.Next();

        CustomTag = customTag;

        ValidateArguments();
    }

    public abstract string CommandName { get; }

    public string CustomTag { get; set; }

    public abstract string[] RequiredArguments { get; }

    public virtual bool ValidateArguments()
    {
        SelfCheck();
        foreach (var argName in RequiredArguments)
        {
            JToken tok;
            if (!arguments.TryGetValue(argName, out tok))
                throw new APICommandConstructionException("Arguments of [" + commandName + "] Command must contain \"" +
                                                          argName + "\" field!");
        }

        return true;
    }

    public virtual string ToJSONString()
    {
        var obj = new JSONObject();
        obj.Add("command", commandName);
        obj.Add("prettyPrint", prettyPrint);
        obj.Add("arguments", arguments);
        obj.Add("customTag", CustomTag);
        return obj.ToString();
    }

    private void SelfCheck()
    {
        if (commandName == null) throw new APICommandConstructionException("commandName cannot be null");

        if (arguments == null) throw new APICommandConstructionException("arguments cannot be null");
    }
}