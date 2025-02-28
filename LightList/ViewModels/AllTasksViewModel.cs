using System.Collections.ObjectModel;
using LightList.Services;

namespace LightList.ViewModels;

public class AllTasksViewModel: BaseTasksViewModel
{ 
    public AllTasksViewModel(ITaskViewModelFactory taskViewModelFactory, ITasksService tasksService) : base(taskViewModelFactory, tasksService)
    {
        AllTasks = new ObservableCollection<TaskViewModel>(TasksService.GetTasks().Select(n => TaskViewModelFactory.Create(n)));
    }
}