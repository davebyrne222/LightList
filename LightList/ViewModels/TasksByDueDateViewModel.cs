using System.Collections.ObjectModel;
using LightList.Services;

namespace LightList.ViewModels;

public class TasksByDueDateViewModel: BaseTasksViewModel
{ 
    public TasksByDueDateViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService) : base(taskViewModelFactory, tasksService)
    {
        AllTasks = new ObservableCollection<TaskViewModel>(TasksService.GetTasks().Select(n => TaskViewModelFactory.Create(n)));
    }
}