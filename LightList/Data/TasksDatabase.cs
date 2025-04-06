using LightList.Utils;
using SQLite;

namespace LightList.Data;

public class TasksDatabase
{
    private readonly ILogger _logger;
    private SQLiteAsyncConnection? _database;

    public TasksDatabase(ILogger logger)
    {
        _logger = logger;
    }

    private SQLiteAsyncConnection Database => _database ??=
        new SQLiteAsyncConnection(
            Constants.DatabasePath,
            Constants.DbFlags
        );

    public async Task InitialiseAsync()
    {
        _logger.Debug("Creating tables");
        var result = await Database.CreateTableAsync<Models.Task>();
        _logger.Debug($"Tables: {result}");
    }

    /**
     * Gets all tasks in database
     */
    public async Task<List<Models.Task>> GetItemsAsync(bool excludeSynced = false, bool excludeDeleted = true)
    {
        _logger.Debug($"Retrieving all tasks (excludeSynced: {excludeSynced}, excludeDeleted: {excludeDeleted})");

        var query = Database.Table<Models.Task>();

        if (excludeSynced)
            query = query.Where(task => task.IsSynced == false);

        if (excludeDeleted)
            query = query.Where(task => task.IsDeleted == false);

        // Sort
        query = query
            .OrderBy(t => t.CompleteAt)
            .OrderBy(t => t.IsCompleted)
            .OrderBy(t => t.DueAt);

        // Execute query
        var tasks = await query.ToListAsync();
        _logger.Debug($"Retrieved {tasks.Count} tasks");
        return tasks;
    }
    
    public async Task<Models.Task> GetItemByIdAsync(string id)
    {
        _logger.Debug($"Retrieving task (id={id})");
        return await Database.Table<Models.Task>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    private async Task<bool> TaskExistsAsync(string id)
    {
        var count = await Database.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {nameof(Models.Task)} WHERE Id = ?", id);
        return count > 0;
    }

    public async Task<string> SaveItemAsync(Models.Task item)
    {
        _logger.Debug($"Storing task id={item.Id}");

        int nRowsUpdated;

        try
        {
            if (await TaskExistsAsync(item.Id))
            {
                _logger.Debug("Task already exists. Updating");
                nRowsUpdated = await Database.UpdateAsync(item);
            }
            else
            {
                _logger.Debug("Task is new. Adding");
                nRowsUpdated = await Database.InsertAsync(item);
            }
        }
        catch (SQLiteException ex)
        {
            _logger.Error($"Database error on save: {ex.GetType()} - {ex.Message}");
            throw;
        }

        if (nRowsUpdated == 0)
            throw new Exception("Task not saved. Unknown reason");

        return item.Id;
    }

    public async Task<int> DeleteItemAsync(Models.Task item)
    {
        _logger.Debug($"Deleting task (id={item.Id})");
        return await Database.DeleteAsync(item);
    }
}