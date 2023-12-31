namespace StrategyApi.StrategyBackgroundService.Dto;

public class StrategyFileDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public byte[] Data { get; set; }
    public DateTime LastDateUpdate { get; set; }
}