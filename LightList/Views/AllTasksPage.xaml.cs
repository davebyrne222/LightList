using LightList.ViewModels;

namespace LightList.Views;

public partial class AllTasksPage : ContentPage
{
    public AllTasksPage(AllTasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}