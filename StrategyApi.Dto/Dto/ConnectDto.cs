using System.ComponentModel.DataAnnotations;

namespace StrategyApi.Dto.Dto;

public class ConnectDto
{
    [Required] public string User { get; set; }
    [Required] public string Pwd { get; set; }
}