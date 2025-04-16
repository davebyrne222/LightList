using LightList.Models;
using Task = System.Threading.Tasks.Task;

namespace LightList.Repositories;

public interface ILocalRepository
{
    Task<List<Models.Task>> GetAllTasks(bool onlyNotSynced = false, bool excludeDeleted = true);

    Task<Models.Task> GetTask(string id);

    Task<int> SaveTask(Models.Task task);

    Task<int> DeleteTask(Models.Task task);

    Task<List<DateOnly>> GetDueDates();

    Task<List<Models.Label>> GetAllLabels(bool excludeSynced = false, bool excludeDeleted = true);
    
    Task<int> SaveLabel(Models.Label label);

    Task<int> DeleteLabel(Models.Label label);
}