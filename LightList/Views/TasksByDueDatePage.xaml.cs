using LightList.ViewModels;

namespace LightList.Views;

public partial class TasksByDueDatePage : ContentPage
{
    public TasksByDueDatePage(TasksByDueDateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TasksByDueDateViewModel vm)
            await vm.OnAppearing();
    }
}