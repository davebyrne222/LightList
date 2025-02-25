namespace LightList.Models;

public class Task
{
    public string Filename { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }
    
    public Task()
    {
        Filename = $"{Path.GetRandomFileName()}.task.txt";
        Date = DateTime.Now;
        Text = "";
    }
}