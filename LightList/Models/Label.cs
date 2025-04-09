using SQLite;

namespace LightList.Models;

public class Label
{
    [PrimaryKey] public string Name { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsSynced { get; set; }
    public bool IsDeleted { get; set; }
}