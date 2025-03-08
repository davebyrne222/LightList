using System.Text.Json;
using LightList.Data;
using LightList.Utils;

namespace LightList.Repositories;

public class LocalRepository: ILocalRepository
{
    private readonly TasksDatabase _database;

    public LocalRepository(TasksDatabase database)
    {
        _database = database;
    }
    
    public async Task<List<Models.Task>> GetAll()
    {
        Logger.Log("Getting all tasks");
        return await _database.GetItemsAsync();
    }
    
    public async Task<Models.Task> Get(int id)
    {
        Logger.Log($"Getting task (id={id})");
        return await _database.GetItemAsync(id);
    }

    public async Task<int> Save(Models.Task task)
    {
        Logger.Log($"Saving task (id={task.Id}, Default Id: {task.Id == default})");
        task.UpdatedOnDate = DateTime.Now;
        return await _database.SaveItemAsync(task);
    }

    public async void Delete(Models.Task task)
    {
        Logger.Log($"Deleting task (id={task.Id})");
        await _database.DeleteItemAsync(task);
    }
}