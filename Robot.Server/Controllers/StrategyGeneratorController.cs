using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Robot.Server.Dto.Response;
using Robot.Server.Services;

namespace Robot.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StrategyGeneratorController(IStrategyGeneratorService strategyGeneratorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateNewStrategy([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier vide ou non fourni.");
        }

        string content;
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            content = await reader.ReadToEndAsync();
        }

        var result = await strategyGeneratorService.CreateNewStrategy(content);
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
    public async Task<IActionResult> UpdateStrategyFile([FromRoute] int id, [FromBody] string file)
    {
        var updatedStrategy = await strategyGeneratorService.UpdateStrategyFile(id, file);
        return Ok(updatedStrategy);
    }


    [HttpGet("GetTemplate")]
    public async Task<IActionResult> GetTemplate()
    {
        var filePath = "Services/Template/StrategyBaseTemplate.cs";
        if (System.IO.File.Exists(filePath))
        {
            var content = await System.IO.File.ReadAllTextAsync(filePath);
            return Ok(new StrategyFileDto
            {
                Data = content,
                Name = "StrategyBaseTemplate"
            });
        }

        return NotFound("Template file not found.");
    }
}