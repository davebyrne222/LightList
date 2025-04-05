using LightList.Data;
using LightList.Utils;
using Task = LightList.Models.Task;

namespace LightList.Repositories;

public class LocalRepository : ILocalRepository
{
    private readonly TasksDatabase _database;
    private readonly ILogger _logger;

    public LocalRepository(TasksDatabase database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<List<Task>> GetAll(bool onlyNotSynced = false, bool excludeDeleted = true)
    {
        _logger.Debug($"Getting all tasks (onlyNotSynced: {onlyNotSynced}, excludeDeleted: {excludeDeleted})");

        return onlyNotSynced switch
        {
            true => await _database.GetNotSyncedAsync(),
            false => await _database.GetItemsAsync(excludeDeleted)
        };
    }

    public async Task<Task> Get(string id)
    {
        _logger.Debug($"Getting task (id={id})");
        return await _database.GetItemByIdAsync(id);
    }

    public async Task<string> Save(Task task)
    {
        _logger.Debug($"Saving task id={task.Id}");
        task.UpdatedAt = DateTime.Now;
        task.IsSynced = false;
        return await _database.SaveItemAsync(task);
    }

    public async void Delete(Task task)
    {
        _logger.Debug($"Deleting task (id={task.Id})");
        await _database.DeleteItemAsync(task);
    }
}