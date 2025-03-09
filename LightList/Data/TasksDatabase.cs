using System.Diagnostics;
using LightList.Utils;
using SQLite;

namespace LightList.Data;

public class TasksDatabase
{
    private readonly SQLiteAsyncConnection? _database;

    public async Task InitialiseAsync()
    {
        Logger.Log("Creating tables");
        var result = await _database!.CreateTableAsync<Models.Task>();
        Logger.Log($"Tables: {result}");
    }

    public TasksDatabase()
    {
        Logger.Log("Initialing database connection");
        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
    }

    public async Task<List<Models.Task>> GetItemsAsync()
    {
        Logger.Log("Retrieving all tasks");
        return await _database.Table<Models.Task>()
            .OrderBy(t => t.Complete)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<Models.Task> GetItemAsync(int id)
    {
        Logger.Log($"Retrieving task (id={id})");
        return await _database.Table<Models.Task>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveItemAsync(Models.Task item)
    {
        Logger.Log($"Saving task (id={item.Id})");
        
        if (item.Id != 0)
        {
            await _database.UpdateAsync(item);
        }
        else
        {
            await _database.InsertAsync(item);
        }
        
        return item.Id;
    }

    public async Task<int> DeleteItemAsync(Models.Task item)
    {
        Logger.Log($"Deleting task (id={item.Id})");
        return await _database.DeleteAsync(item);
    }
}