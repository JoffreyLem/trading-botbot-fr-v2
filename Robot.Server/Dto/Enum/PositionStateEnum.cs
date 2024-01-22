using System.Text.Json.Serialization;

namespace Robot.Server.Dto.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionStateEnum
{
    Opened,
    Updated,
    Closed,
    Rejected
}