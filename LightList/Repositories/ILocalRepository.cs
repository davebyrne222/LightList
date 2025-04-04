using Task = LightList.Models.Task;

namespace LightList.Repositories;

public interface ILocalRepository
{
    Task<List<Task>> GetAll(bool onlyNotSynced = false);

    Task<Task> Get(string id);

    Task<string> Save(Task task);

    void Delete(Task task);
}