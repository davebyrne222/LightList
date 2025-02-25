using LightList.ViewModels;

namespace LightList.Views;

public partial class AllTasksPage : ContentPage
{
    public AllTasksPage(TasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    private void ContentPage_NavigatedTo(object sender, NavigatedToEventArgs e)
    {
        TasksCollection.SelectedItem = null;
    }
}