using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Robot.Server.Dto.Response;
using Robot.Server.Services;

namespace Robot.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StrategyGeneratorController(IStrategyGeneratorService strategyGeneratorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateNewStrategy([FromBody] string file)
    {
        var result = await strategyGeneratorService.CreateNewStrategy(file);
        return Ok(result);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAllStrategyFile()
    {
        var strategies = await strategyGeneratorService.GetAllStrategyFile();
        return Ok(strategies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStrategy(int id)
    {
        var strategies = await strategyGeneratorService.GetStrategyFile(id);
        return Ok(strategies);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStrategyFile(int id)
    {
        await strategyGeneratorService.DeleteStrategyFile(id);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStrategyFile([FromBody] StrategyFileDto strategyFile)
    {
        var updatedStrategy = await strategyGeneratorService.UpdateStrategyFile(strategyFile);
        return Ok(updatedStrategy);
    }
}