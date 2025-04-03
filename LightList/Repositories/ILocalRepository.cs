using LightList.Models;

namespace LightList.Repositories;

public interface ILocalRepository
{
    Task<List<Models.Task>> GetAll(bool onlyNotSynced = false);

    Task<Models.Task> Get(int id);

    Task<int> Save(Models.Task task);
    
    void Delete(Models.Task task);
}