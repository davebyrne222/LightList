using SQLite;

namespace LightList.Models;

public class Task
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Text { get; set; }
    public string Label { get; set; }
    public DateTime CreateOnDate { get; private set; } = DateTime.Now;
    public DateTime UpdatedOnDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; } = DateTime.Now;
    public bool Complete { get; set; }
}