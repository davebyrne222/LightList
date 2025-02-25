using System.Text.Json;

namespace LightList.Repositories;

public class LocalRepository: ILocalRepository
{
    private const string FileExtension = ".lighttask.json";
    
    public IEnumerable<Models.Task> GetAll()
    {
        string appDataPath = FileSystem.AppDataDirectory;

        return Directory
            .EnumerateFiles(appDataPath, $"*{FileExtension}")
            .Select(filename => Get(Path.GetFileName(filename)))
            .OrderBy(task => task.DueDate);
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