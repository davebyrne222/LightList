using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface IRemoteRepository
{
    Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken);
    Task PushUserTasks(AuthTokens accessToken, List<Models.Task> tasks);
}