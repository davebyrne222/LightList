using LightList.ViewModels;

namespace LightList.Views;

public partial class TasksByLabelPage : ContentPage
{
    public TasksByLabelPage(TasksByLabelViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TasksByLabelViewModel vm)
            await vm.OnAppearing();
    }
}