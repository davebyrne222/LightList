using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using LightList.Messages;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class AllTasksViewModel : BaseTasksViewModel
{
    private readonly ILogger _logger;

    public AllTasksViewModel(
        ITaskViewModelFactory taskViewModelFactory,
        ITasksService tasksService,
        IMessenger messenger,
        ILogger logger
    ) : base(taskViewModelFactory, tasksService, messenger, logger)
    {
        _logger = logger;
        // _ = SetTasks();
        Task.Run(async () => await SetTasks()).Wait();
        Messenger.Register<TasksSyncedMessage>(this, async (recipient, _) => { await OnTasksSynced(); });
    }

    private async Task SetTasks()
    {
        _logger.Debug("Updating all tasks");
        var tasks = await TasksService.GetTasks();
        AllTasks = new ObservableCollection<TaskViewModel>(tasks.Select(n => TaskViewModelFactory.Create(n)));
        _logger.Debug($"Retrieved {AllTasks.Count} tasks");
    }

    private async Task OnTasksSynced()
    {
        _logger.Debug("Updating all tasks");
        await SetTasks();
    }
}