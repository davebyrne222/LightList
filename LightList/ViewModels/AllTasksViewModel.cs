using CommunityToolkit.Mvvm.Messaging;
using LightList.Services;
using LightList.Utils;

namespace LightList.ViewModels;

public class AllTasksViewModel : BaseTasksViewModel
{
    private readonly ILogger _logger;
    private bool _hasInitialized; // prevent retrieving data everytime page is navigated to

    public AllTasksViewModel(
        ITaskViewModelFactory taskViewModelFactory,
        ITasksService tasksService,
        IMessenger messenger,
        ILogger logger
    ) : base(taskViewModelFactory, tasksService, messenger, logger)
    {
        _logger = logger;
    }

    public new async Task OnNavigatedTo()
    {
        if (_hasInitialized)
            return;

        _logger.Debug("OnAppearing");
        await base.OnNavigatedTo();
        _hasInitialized = true;
    }
}