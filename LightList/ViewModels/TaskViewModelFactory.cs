using CommunityToolkit.Mvvm.Messaging;
using LightList.Repositories;
using LightList.Services;

namespace LightList.ViewModels;

public class TaskViewModelFactory: ITaskViewModelFactory
{
    private readonly ITasksService _tasksService;
    private readonly IMessenger _messenger;

    public TaskViewModelFactory(ITasksService tasksService, IMessenger messenger)
    {
        _tasksService = tasksService;
        _messenger = messenger;
    }

    public TaskViewModel Create(Models.Task task)
    {
        return new TaskViewModel(_tasksService, _messenger, task);
    }
}