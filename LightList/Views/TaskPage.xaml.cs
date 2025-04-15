using LightList.ViewModels;

namespace LightList.Views;

public partial class TaskPage : ContentPage
{
    public TaskPage(TaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    private async void OnNewLabelButtonClicked(object sender, EventArgs e)
    {
        var result = await DisplayPromptAsync("New Label", "", placeholder: "Label");
        if (!string.IsNullOrWhiteSpace(result))
        {
            var vm = BindingContext as TaskViewModel;
            vm?.AddLabelAsync(result);
        }
    }
}