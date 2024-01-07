using System.ComponentModel.DataAnnotations;

namespace StrategyApi.StrategyBackgroundService.Dto;

public class ConnectDto
{
    [Required] public string User { get; set; }
    [Required] public string Pwd { get; set; }
}