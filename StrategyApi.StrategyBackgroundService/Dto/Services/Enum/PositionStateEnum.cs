using System.Text.Json.Serialization;

namespace StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PositionStateEnum
{
    Opened,
    Updated,
    Closed,
    Rejected
}