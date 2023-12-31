using System.Text.Json.Serialization;

namespace RobotAppLibraryV2.ApiHandler.Handlers.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiHandlerEnum
{
    Xtb = 0
}