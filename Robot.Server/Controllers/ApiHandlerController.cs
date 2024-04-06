using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Robot.Server.Dto.Response;
using Robot.Server.Services;

namespace Robot.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApiHandlerController(IApiConnectService apiConnectService) : ControllerBase
{
    [HttpPost("connect")]
    public async Task<IActionResult> Connect([FromBody] ConnectDto connectDto)
    {
        await apiConnectService.Connect(connectDto);
        return Ok();
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        await apiConnectService.Disconnect();
        return Ok();
    }

    [HttpGet("isConnected")]
    public async Task<IActionResult> IsConnected()
    {
        var connectionState = await apiConnectService.IsConnected();
        return Ok(new ApiResponse<bool>()
        {
            Data = connectionState,
        });
    }

    [HttpGet("typeHandler")]
    public async Task<IActionResult> GetTypeHandler()
    {
        var typeHandler = await apiConnectService.GetTypeHandler();
        return Ok(new ApiResponse<string>()
        {
            Data = typeHandler
        });
    }

    [HttpGet("listHandlers")]
    public async Task<IActionResult> GetListHandler()
    {
        var listHandlers = await apiConnectService.GetListHandler();
        return Ok(listHandlers);
    }

    [HttpGet("allSymbols")]
    public async Task<IActionResult> GetAllSymbol()
    {
        var allSymbols = await apiConnectService.GetAllSymbol();
        return Ok(allSymbols);
    }
}