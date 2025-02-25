using LightList.Models;

namespace LightList.Services;

public interface ITasksService
{
    Models.Task GetTask(string filename);
    
    IEnumerable<Models.Task> GetTasks();

    void SaveTask(Models.Task task);
    
    void DeleteTask(Models.Task task);
}