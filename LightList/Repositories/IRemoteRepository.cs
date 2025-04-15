using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface IRemoteRepository
{
    Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken, DateTime? lastSyncDate);
    Task PushUserTask(AuthTokens accessToken, Models.Task tasks);

    Task PushUserLabel(AuthTokens accessToken, Models.Label label);
    Task<List<Models.Label?>> GetUserLabels(AuthTokens accessToken, DateTime? lastSyncDate);
}