using LightList.Utils;
using SQLite;

namespace LightList.Data;

public class TasksDatabase
{
    private SQLiteAsyncConnection? _database;
    private SQLiteAsyncConnection Database => _database ??= new SQLiteAsyncConnection(Constants.DatabasePath, Constants.DbFlags);
    public async Task InitialiseAsync()
    {
        Logger.Log("Creating tables");
        var result = await Database!.CreateTableAsync<Models.Task>();
        Logger.Log($"Tables: {result}");
    }
    public TasksDatabase() { }
    public async Task<List<Models.Task>> GetItemsAsync()
    {
        Logger.Log("Retrieving all tasks");
        
        return await Database.Table<Models.Task>()
            .OrderBy(t => t.CompleteOnDate)
            .OrderBy(t => t.Complete)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }
    public async Task<Models.Task> GetItemAsync(int id)
    {
        Logger.Log($"Retrieving task (id={id})");
        return await Database.Table<Models.Task>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }
    public async Task<int> SaveItemAsync(Models.Task item)
    {
        Logger.Log($"Saving task (id={item.Id})");
        
        if (item.Id != 0)
        {
            await Database.UpdateAsync(item);
        }
        else
        {
            await Database.InsertAsync(item);
        }
        
        return item.Id;
    }
    public async Task<int> DeleteItemAsync(Models.Task item)
    {
        Logger.Log($"Deleting task (id={item.Id})");
        return await Database.DeleteAsync(item);
    }
}