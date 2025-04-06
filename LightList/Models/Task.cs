using SQLite;

namespace LightList.Models;

public class Task
{
    [PrimaryKey] public string Id { get; init; } = Guid.NewGuid().ToString();
    [NotNull] public string Text { get; set; } = string.Empty;
    public string? Label { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime DueAt { get; set; } = DateTime.Now;
    public DateTime? CompleteAt { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsSynced { get; set; }
    public bool IsDeleted { get; set; }
}