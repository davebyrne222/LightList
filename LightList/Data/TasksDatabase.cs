using LightList.Utils;
using SQLite;

namespace LightList.Data;

public class TasksDatabase
{
    private SQLiteAsyncConnection? _database;
    private SQLiteAsyncConnection Database => _database ??= 
        new SQLiteAsyncConnection(
            Constants.DatabasePath, 
            Constants.DbFlags,
            true
         );
    public async Task InitialiseAsync()
    {
        Logger.Log("Creating tables");
        var result = await Database!.CreateTableAsync<Models.Task>();
        Logger.Log($"Tables: {result}");
    }
    public TasksDatabase() { }
    
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
    
    public async Task<Models.Task> GetItemByIdAsync(int id)
    {
        Logger.Log($"Retrieving task (id={id})");
        return await Database.Table<Models.Task>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }
    
    private async Task<bool> TaskExistsAsync(string uid)
    {
        var task = await Database.Table<Models.Task>()
            .Where(t => t.Uid == uid)
            .FirstOrDefaultAsync();
        return task != null;
    }
    
    public async Task<int> SaveItemAsync(Models.Task item)
    {
        Logger.Log($"Saving task (id={item.Id}, uid={item.Uid})");

        int nRowsUpdated;

        bool taskExist = await TaskExistsAsync(item.Uid);
            
        if (taskExist)
        {
            Logger.Log($"Task already exists. Updating");
            nRowsUpdated = await Database.UpdateAsync(item);
        }
        else
        {
            Logger.Log($"Task is new. Adding");
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