namespace LightList.Models;

public class Task
{
    public string Id { get; set; }
    public string Text { get; set; }
    public string Label { get; set; }
    public DateTime CreateOnDate { get; set; }
    public DateTime UpdatedOnDate { get; set; }
    public DateTime DueDate { get; set; }
    
    public Task()
    {
        // Id = $"{Path.GetRandomFileName()}.task.txt";
        Id = "";
        Text = "";
        Label = "";
        CreateOnDate = DateTime.Now;
        UpdatedOnDate = DateTime.Now;
        DueDate = DateTime.Now;
    }
}