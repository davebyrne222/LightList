using Task = System.Threading.Tasks.Task;

namespace LightList.Services;

public interface ISyncService
{
    Task<List<Models.Task?>> PullTasksAsync();
    Task PushTasksAsync(List<Models.Task> tasks);
    Task PushLabelsAsync(List<Models.Label> labels);
    Task<List<Models.Label?>> PullLabelsAsync();
}