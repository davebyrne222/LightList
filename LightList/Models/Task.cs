using System.Runtime.CompilerServices;
using SQLite;

namespace LightList.Models;

public class Task
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [NotNull]
    public string Text { get; set; } = string.Empty;
    public string Label { get; set; } = "-";
    public DateTime CreateOnDate { get; private set; } = DateTime.Now;
    public DateTime UpdatedOnDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; } = DateTime.Now;
    public DateTime? CompleteOnDate { get; set; }
    public bool Complete { get; set; }
}