using LightList.Data;
using LightList.Utils;
using Task = LightList.Models.Task;

namespace LightList.Repositories;

public class LocalRepository : ILocalRepository
{
    private readonly TasksDatabase _database;

    public LocalRepository(TasksDatabase database)
    {
        _database = database;
    }

    public async Task<List<Task>> GetAll(bool onlyNotSynced = false)
    {
        Logger.Log($"Getting all tasks (only not synced: {onlyNotSynced})");

        return onlyNotSynced switch
        {
            true => await _database.GetNotSyncedAsync(),
            false => await _database.GetItemsAsync()
        };
    }

    public async Task<Task> Get(string id)
    {
        Logger.Log($"Getting task (id={id})");
        return await _database.GetItemByIdAsync(id);
    }

    public async Task<string> Save(Task task)
    {
        Logger.Log($"Saving task id={task.Id}");
        task.UpdatedOnDate = DateTime.Now;
        task.IsPushedToRemote = false;
        return await _database.SaveItemAsync(task);
    }

    public async void Delete(Task task)
    {
        Logger.Log($"Deleting task (id={task.Id})");
        await _database.DeleteItemAsync(task);
    }
}