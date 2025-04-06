using Task = LightList.Models.Task;

namespace LightList.Services;

public interface ISyncService
{
    Task<List<Task?>> PullChangesAsync();
    System.Threading.Tasks.Task PushChangesAsync(List<Task> tasks);
}