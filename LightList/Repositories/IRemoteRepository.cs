namespace LightList.Repositories;

public interface IRemoteRepository
{
    Task<List<Models.Task?>> ExecuteQuery(string idToken, string query);
}