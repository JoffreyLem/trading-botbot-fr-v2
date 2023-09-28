using Destructurama.Attributed;

namespace StrategyApi.StrategyBackgroundService.Dto.Command.Api;

public class ApiConnectCommandDto : ApiCommandBaseDto
{
    public string User { get; set; }

    [LogMasked]
    public string Password { get; set; }
}