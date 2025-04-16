using LightList.ViewModels;

namespace LightList.Views;

public partial class TasksByDueDatePage : ContentPage
{
    public TasksByDueDatePage(TasksByDueDateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is TasksByDueDateViewModel vm)
            await vm.OnNavigatedTo();
    }
}