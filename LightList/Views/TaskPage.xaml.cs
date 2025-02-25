using LightList.ViewModels;

namespace LightList.Views;

public partial class TaskPage : ContentPage
{
    public TaskPage(TaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}