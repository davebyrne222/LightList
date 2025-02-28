using LightList.ViewModels;

namespace LightList.Views;

public partial class TasksByDueDatePage : ContentPage
{
    public TasksByDueDatePage(TasksByDueDateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}