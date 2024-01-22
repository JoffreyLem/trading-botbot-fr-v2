using System.ComponentModel.DataAnnotations;

namespace Robot.DataBase.Modeles;

public class StrategyFile
{
    [Key] public int Id { get; set; }

    public string Name { get; set; }
    public string Version { get; set; }
    public byte[] Data { get; set; }
    public DateTime LastDateUpdate { get; set; }
}