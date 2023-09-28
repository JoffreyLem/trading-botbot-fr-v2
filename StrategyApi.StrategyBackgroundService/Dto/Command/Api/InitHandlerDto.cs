using StrategyApi.Dto.Enum;

namespace StrategyApi.StrategyBackgroundService.Dto.Command.Api;

public class InitHandlerCommandDto : ApiCommandBaseDto
{
    public ApiHandlerEnum? ApiHandlerEnum { get; set; }
}