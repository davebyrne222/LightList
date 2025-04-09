using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface ILocalRepository
{
    Task<List<Models.Task>> GetAllTasks(bool onlyNotSynced = false, bool excludeDeleted = true);

    Task<Models.Task> GetTask(string id);

    Task<int> SaveTask(Models.Task task);

    Task<int> DeleteTask(Models.Task task);

    Task<List<Models.Label>> GetAllLabels();
    
    Task<int> SaveLabel(Models.Label label);

    Task<int> DeleteLabel(Models.Label label);
}