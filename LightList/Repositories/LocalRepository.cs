using LightList.Models;

namespace LightList.Repositories;

public class LocalRepository: ILocalRepository
{
    public IEnumerable<Models.Task> GetAll()
    {
        // return await _dbContext.Items.ToListAsync();
        
        // Get the folder where the tasks are stored.
        string appDataPath = FileSystem.AppDataDirectory;

        // Use Linq extensions to load the *.tasks.txt files.
        return Directory

            // Select the file names from the directory
            .EnumerateFiles(appDataPath, "*.task.txt")

            // Each file name is used to load a task
            .Select(filename => Get(Path.GetFileName(filename)))

            // With the final collection of tasks, order them by date
            .OrderByDescending(task => task.Date);
    }
    
    public Models.Task Get(string filename)
    {
        filename = Path.Combine(FileSystem.AppDataDirectory, filename);

        if (!File.Exists(filename))
            throw new FileNotFoundException("Unable to find file on local storage.", filename);

        return
            new Models.Task
            {
                Filename = Path.GetFileName(filename),
                Text = File.ReadAllText(filename),
                Date = File.GetLastWriteTime(filename)
            };
    }

    public void Save(Models.Task task) =>
        File.WriteAllText(System.IO.Path.Combine(FileSystem.AppDataDirectory, task.Filename), task.Text);

    public void Delete(Models.Task task) =>
        File.Delete(System.IO.Path.Combine(FileSystem.AppDataDirectory, task.Filename));
}