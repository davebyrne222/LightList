using SQLite;

namespace LightList.Models;

public class Task
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Text { get; set; }
    public string Label { get; set; }
    public DateTime CreateOnDate { get; set; } = DateTime.Now;
    public DateTime UpdatedOnDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; } = DateTime.Now;
    
    // public Task()
    // {
    //     // Id = $"{Path.GetRandomFileName()}.task.txt";
    //     // Id = "";
    //     Text = "";
    //     Label = "";
    //     CreateOnDate = DateTime.Now;
    //     UpdatedOnDate = DateTime.Now;
    //     DueDate = DateTime.Now;
    // }
}