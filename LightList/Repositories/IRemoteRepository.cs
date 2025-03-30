using LightList.Models;

namespace LightList.Repositories;

public interface IRemoteRepository
{
    Task<List<Models.Task?>> GetUserTasks(AuthTokens accessToken);
}