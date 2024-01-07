using System.Text.Json.Serialization;

namespace StrategyApi.StrategyBackgroundService.Dto.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionStateEnum
{
    Opened,
    Updated,
    Closed,
    Rejected
}