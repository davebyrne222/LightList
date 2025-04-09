using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public interface ITasksService
{
    Task<Models.Task> GetTask(string taskId);

    Task<List<Models.Task>> GetTasks();

    Task SaveTask(Models.Task task);

    Task DeleteTask(Models.Task task);

    Task SyncNowAsync();

    Task<List<Models.Label>> GetLabels();

    Task SaveLabel(Models.Label label);

    Task DeleteLabel(Models.Label label);
}