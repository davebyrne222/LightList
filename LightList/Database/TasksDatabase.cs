using System.Reflection;
using LightList.Utils;
using SQLite;
using Task = System.Threading.Tasks.Task;

namespace LightList.Database;

public class TasksDatabase
{
    private static readonly List<string> Tables =
    [ // N.B: Order is important!
        "LightList.Database.Scripts.CreateLabelTable.sql",
        "LightList.Database.Scripts.CreateTaskTable.sql",
        "LightList.Database.Scripts.CreateIndexes.sql"
    ];

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

        // Enable FKs; SQLite does not enable by default
        // await Database.ExecuteAsync("DROP TABLE IF EXISTS Task;");
        // await Database.ExecuteAsync("DROP TABLE IF EXISTS Label;");
        await Database.ExecuteAsync("PRAGMA foreign_keys = ON;");

        // Create tables:
        foreach (var table in Tables)
        {
            var result = await ExecuteSqlFromFileAsync(table);
            _logger.Debug($"Table '{table}' created: {result} (no. rows affected)");
        }
    }

    #region Public methods - Tasks

    /**
     * Gets all tasks in database
     */
    public async Task<List<Models.Task>> GetTasksAsync(bool excludeSynced = false, bool excludeDeleted = true)
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

    public async Task<Models.Task> GetTaskByIdAsync(string id)
    {
        _logger.Debug($"Retrieving task (id={id})");
        return await Database.Table<Models.Task>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveTaskAsync(Models.Task item)
    {
        _logger.Debug($"Storing task id={item.Id}");

        if (await ItemExistsAsync(nameof(Models.Task), "Id", item.Id))
        {
            _logger.Debug("Task already exists. Updating");
            return await Database.UpdateAsync(item);
        }
        
        return await Database.InsertAsync(item);
    }

    public async Task<int> DeleteTaskAsync(Models.Task item)
    {
        _logger.Debug($"Deleting task (id={item.Id})");
        return await Database.DeleteAsync(item);
    }

    public class DueDateWrapper
    {
        [Column("DueDate")]
        public DateTime DueDate { get; set; }
    }
    
    /**
     * Using SQLite DATE function and sqlite-net-pcl returns default
     * value and so using DISTINCT results in only a default date being returned
     *
     * Issue raised at: https://github.com/praeclarum/sqlite-net/issues/1275
     *
     * Until resolved, must use workaround
     *
     * TODO: fix
     */
    
    public async Task<List<DateOnly>> GetUniqueDueDatesAsync()
    {
        _logger.Debug("Retrieving unique due dates");
        
        // **N.B**: See bug described above
        //
        // var dueDates = await Database.QueryAsync<DueDateWrapper>(
        //     "SELECT DISTINCT DATE(DueAt) AS DueDate FROM Task"
        // );
        //
        // List<DateOnly> uniqueDueDates = dueDates.Select(d => DateOnly.FromDateTime(d.DueDate)).ToList();
        
        // BUG WORKAROUND:
        // ---
        var dueDates = await Database.QueryAsync<DueDateWrapper>(
            "SELECT DISTINCT DueAt AS DueDate FROM Task WHERE IsDeleted = 0"
        );
        
        var uniqueDueDates = new HashSet<DateOnly>(
            dueDates.Select(d => DateOnly.FromDateTime(d.DueDate))
        ).ToList();
        // ---
        
        _logger.Debug($"Retrieved {uniqueDueDates.Count} unique dates");
        
        return uniqueDueDates;
    }

    #endregion
    
    #region Public methods - Labels

    public async Task<List<Models.Label>> GetLabelsAsync(bool excludeSynced = false, bool excludeDeleted = true)
    {
        _logger.Debug($"Retrieving all labels (excludeSynced: {excludeSynced}, excludeDeleted: {excludeDeleted})");

        var query = Database.Table<Models.Label>();

        if (excludeSynced)
            query = query.Where(label => label.IsSynced == false);

        if (excludeDeleted)
            query = query.Where(label => label.IsDeleted == false);

        // Sort
        query = query
            .OrderBy(l => l.Name);

        // Execute query
        var labels = await query.ToListAsync();
        _logger.Debug($"Retrieved {labels.Count} labels");
        return labels;
    }

    public async Task<int> SaveLabelAsync(Models.Label label)
    {
        _logger.Debug($"Storing label '{label.Name}'");
        
        if (await ItemExistsAsync(nameof(Models.Label), "Name", label.Name))
        {
            _logger.Debug("Label already exists. Updating");
            return await Database.UpdateAsync(label);
        }
        
        return await Database.InsertAsync(label);
    }
    
    public async Task<int> DeleteLabelAsync(Models.Label label)
    {
        _logger.Debug($"Deleting label '{label.Name}'");
        return await Database.DeleteAsync(label);
    }
    
    #endregion

    #region Utils

    private async Task<bool> ItemExistsAsync(string tableName, string colName, string value)
    {
        var count = await Database.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {tableName} WHERE {colName} = ?", value);
        return count > 0;
    }

    private async Task<int> ExecuteSqlFromFileAsync(string fileName)
    {
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);

        if (stream == null)
            throw new FileNotFoundException($"Embedded SQL resource not found: {fileName}");

        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync();
        return await Database.ExecuteAsync(sql);
    }

    #endregion
}