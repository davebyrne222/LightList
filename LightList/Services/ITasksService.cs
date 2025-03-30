using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public interface ITasksService
{
    Task<Models.Task> GetTask(int taskId);
    
    Task<List<Models.Task>> GetTasks();

    Task<int> SaveTask(Models.Task task);
    
    void DeleteTask(Models.Task task);

    Task SyncNowAsync();
}