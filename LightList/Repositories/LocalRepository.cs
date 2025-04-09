using LightList.Data;
using LightList.Utils;
using LightList.Models;
using Task = System.Threading.Tasks.Task;

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

    public async Task<List<Models.Task>> GetAllTasks(bool excludeSynced = false, bool excludeDeleted = true)
    {
        _logger.Debug($"Getting all tasks (excludeSynced: {excludeSynced}, excludeDeleted: {excludeDeleted})");
        return await _database.GetTasksAsync(excludeSynced, excludeDeleted);
    }

    public async Task<Models.Task> GetTask(string id)
    {
        _logger.Debug($"Getting task (id={id})");
        return await _database.GetTaskByIdAsync(id);
    }

    public async Task<int> SaveTask(Models.Task task)
    {
        _logger.Debug($"Saving task id={task.Id}");
        return await _database.SaveTaskAsync(task);
    }

    public async Task<int> DeleteTask(Models.Task task)
    {
        _logger.Debug($"Deleting task (id={task.Id})");
        return await _database.DeleteTaskAsync(task);
    }
    
    public async Task<List<Models.Label>> GetAllLabels()
    {
        _logger.Debug($"Getting all labels");
        return await _database.GetLabelsAsync();
    }
    
    public async Task<int> SaveLabel(Models.Label label)
    {
        _logger.Debug($"Saving label '{label.Name}'");
        return await _database.SaveLabelAsync(label);
    }

    public async Task<int> DeleteLabel(Models.Label label)
    {
        _logger.Debug($"Deleting label '{label.Name}')");
        return await _database.DeleteLabelAsync(label);
    }
}