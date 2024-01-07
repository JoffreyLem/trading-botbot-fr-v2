using System.Text.Json.Serialization;

namespace StrategyApi.StrategyBackgroundService.Dto.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnexionStateEnum
{
    Connected,
    Disconnected,
    Inconnue,
    Reconnecting,
    NotInitialized,
    Initialized
}