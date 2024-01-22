using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Robot.Server.Dto.Request;
using Robot.Server.Dto.Response;
using Robot.Server.Services;

namespace Robot.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StrategyController(IStrategyHandlerService strategyHandlerService) : ControllerBase
{
    [HttpPost("init")]
    public async Task<IActionResult> InitStrategy([FromBody] StrategyInitDto strategyInitDto)
    {
        await strategyHandlerService.InitStrategy(strategyInitDto);
        return Ok();
    }

    [HttpGet("timeframes")]
    public async Task<ActionResult<List<string>>> GetListTimeframes()
    {
        return await strategyHandlerService.GetListTimeframes();
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<StrategyInfoDto>>> GetAllStrategy()
    {
        return await strategyHandlerService.GetAllStrategy();
    }

    [HttpPost("close/{id}")]
    public async Task<IActionResult> CloseStrategy(string id)
    {
        await strategyHandlerService.CloseStrategy(id);
        return Ok();
    }

    [HttpGet("{id}/info")]
    public async Task<ActionResult<StrategyInfoDto>> GetStrategyInfo(string id)
    {
        return await strategyHandlerService.GetStrategyInfo(id);
    }

    [HttpGet("{id}/positions/closed")]
    public async Task<ActionResult<List<PositionDto>>> GetStrategyPositionClosed(string id)
    {
        return await strategyHandlerService.GetStrategyPositionClosed(id);
    }

    [HttpGet("{id}/result")]
    public async Task<ActionResult<ResultDto>> GetResult(string id)
    {
        return await strategyHandlerService.GetResult(id);
    }

    [HttpGet("{id}/resultBacktest")]
    public async Task<ActionResult<BackTestDto>> GetResultBacktest(string id)
    {
        return await strategyHandlerService.GetBacktestResult(id);
    }

    [HttpPost("{id}/canrun")]
    public async Task<IActionResult> SetCanRun(string id, [FromQuery] bool value)
    {
        await strategyHandlerService.SetCanRun(id, value);
        return Ok();
    }

    [HttpGet("{id}/positions/opened")]
    public async Task<ActionResult<List<PositionDto>>> GetOpenedPositions(string id)
    {
        return await strategyHandlerService.GetOpenedPositions(id);
    }

    [HttpPost("runbacktest/{id}")]
    public async Task<ActionResult<BackTestDto>> RunBackTest(string id,
        [FromBody] BackTestRequestDto backTestRequestDto)
    {
        return await strategyHandlerService.RunBackTest(id, backTestRequestDto);
    }
}