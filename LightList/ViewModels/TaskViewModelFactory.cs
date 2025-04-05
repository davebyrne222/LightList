using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;
using Task = LightList.Models.Task;

namespace LightList.ViewModels;

public class TaskViewModelFactory : ITaskViewModelFactory
{
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    private readonly ITasksService _tasksService;

    public TaskViewModelFactory(ITasksService tasksService, IMessenger messenger, ILogger logger)
    {
        _logger = logger;
        _tasksService = tasksService;
        _messenger = messenger;
    }

    public TaskViewModel Create(Task task)
    {
        return new TaskViewModel(_tasksService, _messenger, _logger, task);
    }
}