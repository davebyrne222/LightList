using CommunityToolkit.Mvvm.Messaging;
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
    }

    public async Task OnAppearing()
    {
        _logger.Debug("OnAppearing");
        await GetTasks();
        await GetLabels();
    }
}