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

    public async Task<List<Task>> GetAll(bool excludeSynced = false, bool excludeDeleted = true)
    {
        _logger.Debug($"Getting all tasks (excludeSynced: {excludeSynced}, excludeDeleted: {excludeDeleted})");
        return await _database.GetItemsAsync(excludeSynced, excludeDeleted);
    }

    public async Task<Task> Get(string id)
    {
        _logger.Debug($"Getting task (id={id})");
        return await _database.GetItemByIdAsync(id);
    }

    public async Task<string> Save(Task task)
    {
        _logger.Debug($"Saving task id={task.Id}");
        
        // TODO: relocate these updates to Task service
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