using System.ComponentModel.DataAnnotations;

namespace StrategyApi.StrategyBackgroundService.Dto.Services;

public class ConnectDto
{
    [Required] public string User { get; set; }
    [Required] public string Pwd { get; set; }
}