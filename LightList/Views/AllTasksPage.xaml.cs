using LightList.ViewModels;

namespace LightList.Views;

public partial class AllTasksPage : ContentPage
{
    public AllTasksPage(AllTasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)

    {
        base.OnNavigatedTo(args);
        if (BindingContext is AllTasksViewModel vm)
            await vm.OnNavigatedTo();
    }
}