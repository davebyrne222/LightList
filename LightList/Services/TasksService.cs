using LightList.Models;
using LightList.Repositories;

namespace LightList.Services;

public class TasksService: ITasksService
{
    private readonly ILocalRepository _localRepository;

    public TasksService(ILocalRepository localRepository)
    {
        _localRepository = localRepository;
    }

    public Models.Task GetTask(string filename)
    {
        return _localRepository.Get(filename);
    }

    public IEnumerable<Models.Task> GetTasks()
    {
        return _localRepository.GetAll();
    }

    public void SaveTask(Models.Task task)
    {
        _localRepository.Save(task);
    }

    public void DeleteTask(Models.Task task)
    {
        _localRepository.Delete(task);
    }
}