using LightList.Utils;
using SQLite;

namespace LightList.Data;

public class TasksDatabase
{
    private SQLiteAsyncConnection? _database;

    private SQLiteAsyncConnection Database => _database ??=
        new SQLiteAsyncConnection(
            Constants.DatabasePath,
            Constants.DbFlags
        );

    public async Task InitialiseAsync()
    {
        Logger.Log("Creating tables");
        var result = await Database!.CreateTableAsync<Models.Task>();
        Logger.Log($"Tables: {result}");
    }

    /**
     * Gets all tasks in database
     */
    public async Task<List<Models.Task>> GetItemsAsync()
    {
        Logger.Log("Retrieving all tasks");

        var tasks = await Database.Table<Models.Task>()
            .OrderBy(t => t.CompleteOnDate)
            .OrderBy(t => t.Complete)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        Logger.Log($"Retrieved {tasks.Count} tasks");

        return tasks;
    }

    /**
     * Gets issues which have not been pushed to remote DB
     */
    public async Task<List<Models.Task>> GetNotSyncedAsync()
    {
        Logger.Log("Retrieving tasks that are not synced");

        var tasks = await Database.Table<Models.Task>()
            .Where(t => t.IsPushedToRemote == false)
            .ToListAsync();

        Logger.Log($"Retrieved {tasks.Count} tasks");

        return tasks;
    }

    public async Task<Models.Task> GetItemByIdAsync(string id)
    {
        Logger.Log($"Retrieving task (id={id})");
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
        Logger.Log($"Storing task id={item.Id}");

        int nRowsUpdated;

        if (await TaskExistsAsync(item.Id))
        {
            Logger.Log("Task already exists. Updating");
            nRowsUpdated = await Database.UpdateAsync(item);
        }
        else
        {
            Logger.Log("Task is new. Adding");
            nRowsUpdated = await Database.InsertAsync(item);
        }

        if (nRowsUpdated == 0)
            throw new Exception("Failed to save task");

        return item.Id;
    }

    public async Task<int> DeleteItemAsync(Models.Task item)
    {
        Logger.Log($"Deleting task (id={item.Id})");
        return await Database.DeleteAsync(item);
    }
}