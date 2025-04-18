using LightList.ViewModels;

namespace LightList.Views;

public partial class TaskPage : ContentPage
{
    public TaskPage(TaskViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnNewLabelButtonClicked(object? sender, EventArgs e)
    {
        var result = await DisplayPromptAsync("New Label", "", placeholder: "Label");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var vm = BindingContext as TaskViewModel;
            await vm?.AddLabelAsync(result)!;
        }
    }

    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Delete Task?",
            "This can not be un-done",
            "Delete",
            "Cancel");

        if (confirm)
        {
            var vm = BindingContext as TaskViewModel;
            vm?.DeleteCommand.Execute(null);
        }
    }
}