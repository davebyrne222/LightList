using System.Text.Json;

namespace LightList.Repositories;

public class LocalRepository: ILocalRepository
{
    private const string FileExtension = ".lighttask.json";
    
    public IEnumerable<Models.Task> GetAll()
    {
        // return await _dbContext.Items.ToListAsync();
        
        // Get the folder where the tasks are stored.
        string appDataPath = FileSystem.AppDataDirectory;

        // Use Linq extensions to load the *.tasks.txt files.
        return Directory

            // Select the file names from the directory
            .EnumerateFiles(appDataPath, $"*{FileExtension}")

            // Each file name is used to load a task
            .Select(filename => Get(Path.GetFileName(filename)))

            // With the final collection of tasks, order them by date
            .OrderByDescending(task => task.CreateOnDate);
    }
    
    public Models.Task Get(string id)
    {
        if (!id.Contains(FileExtension)) id += FileExtension;
        
        string filename = Path.Combine(FileSystem.AppDataDirectory, id);

        if (!File.Exists(filename))
            throw new FileNotFoundException("Unable to find file on local storage.", filename);
        
        string json = File.ReadAllText(filename);
        return JsonSerializer.Deserialize<Models.Task>(json);
    }

    public void Save(Models.Task task)
    {
        task.Id = string.IsNullOrEmpty(task.Id) ? Guid.NewGuid().ToString() : task.Id;

        string filename = Path.Combine(FileSystem.AppDataDirectory, task.Id + FileExtension);
        string json = JsonSerializer.Serialize(task, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(filename, json);
    }

    public void Delete(Models.Task task)
    {
        string filename = Path.Combine(FileSystem.AppDataDirectory, task.Id + FileExtension);
        if (File.Exists(filename))
            File.Delete(filename);
    }
}