using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface IRemoteRepository
{
    Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken, DateTime? lastSyncDate);
    Task PushUserTask(AuthTokens accessToken, Models.Task tasks);
}