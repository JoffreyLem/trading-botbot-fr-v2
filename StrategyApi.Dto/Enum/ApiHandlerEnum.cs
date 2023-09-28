using System.Text.Json.Serialization;

namespace StrategyApi.Dto.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiHandlerEnum
{
    Xtb = 0
}