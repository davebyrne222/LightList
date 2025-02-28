using LightList.ViewModels;

namespace LightList.Views;

public partial class TasksByLabelPage : ContentPage
{
    public TasksByLabelPage(TasksByLabelViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}