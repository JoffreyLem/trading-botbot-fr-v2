using System.ComponentModel.DataAnnotations;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;

namespace Robot.Server.Dto.Response;

public class ConnectDto
{
    public string? User { get; set; }


    public string? Pwd { get; set; }

    [Required] public ApiHandlerEnum? HandlerEnum { get; set; }
}