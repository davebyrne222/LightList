using LightList.ViewModels;

namespace LightList.Views;

public partial class TasksByLabelPage : ContentPage
{
    public TasksByLabelPage(TasksByLabelViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is TasksByLabelViewModel vm)
            await vm.OnNavigatedTo();
    }
}