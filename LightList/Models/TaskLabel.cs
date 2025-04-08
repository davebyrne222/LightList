using SQLite;

namespace LightList.Models;

public class TaskLabel
{
    [PrimaryKey] public required string Name { get; set; }
}