namespace LightList.ViewModels;

public interface ITaskViewModelFactory
{
    TaskViewModel Create(Models.Task task);
}