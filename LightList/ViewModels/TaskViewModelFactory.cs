using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;
using Task = LightList.Models.Task;

namespace LightList.ViewModels;

public class TaskViewModelFactory : ITaskViewModelFactory
{
    private readonly ILogger _logger;
    private readonly LoggerContext _loggerContext;
    private readonly IMessenger _messenger;
    private readonly ITasksService _tasksService;

    public TaskViewModelFactory(LoggerContext loggerContext, ILogger logger, ITasksService tasksService,
        IMessenger messenger)
    {
        _loggerContext = loggerContext;
        _logger = logger;
        _tasksService = tasksService;
        _messenger = messenger;
    }

    public TaskViewModel Create(Task task)
    {
        return new TaskViewModel(_loggerContext, _logger, _tasksService, _messenger, task);
    }
}