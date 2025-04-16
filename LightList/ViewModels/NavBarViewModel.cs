using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using LightList.Utils;
using LightList.Views;

namespace LightList.ViewModels;

public class NavBarViewModel : INotifyPropertyChanged
{
    private readonly ILogger _logger;
    private readonly LoggerContext _loggerContext;
    private string _selectedView = "All";

    public NavBarViewModel(LoggerContext loggerContext, ILogger logger)
    {
        _loggerContext = loggerContext;
        _logger = logger;

        // TODO: replace with AsyncRelayCommand (community toolkit)
        OpenMenuCommand = new Command(OpenMenu);
        AddTaskCommand = new Command(AddTask);
        NavigateCommand = new Command<string>(NavigateToPage);
    }

    public string SelectedView
    {
        get => _selectedView;
        set
        {
            _selectedView = value;
            OnPropertyChanged(nameof(SelectedView));
        }
    }

    public ICommand OpenMenuCommand { get; }
    public ICommand AddTaskCommand { get; }
    public ICommand NavigateCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void OpenMenu()
    {
        Debug.WriteLine("---[NavBarViewModel.OpenMenu] Opening Flyout");
        Shell.Current.FlyoutIsPresented = true;
    }

    private async void AddTask()
    {
        _loggerContext.Group = "Add Task";
        _logger.Debug("Adding new task");

        if (Shell.Current.CurrentPage is TaskPage)
            return;

        await Shell.Current.GoToAsync(nameof(TaskPage));

        _loggerContext.Reset();
    }

    private async void NavigateToPage(string pageName)
    {
        var route = pageName switch
        {
            "All" => nameof(AllTasksPage),
            "Due" => nameof(TasksByDueDatePage),
            "Label" => nameof(TasksByLabelPage),
            _ => null
        };

        if (route == null || Shell.Current.CurrentPage?.GetType().Name == route)
            return;

        // Update styling
        SelectedView = pageName;

        _logger.Debug($"Navigating to {route}");
        await Shell.Current.GoToAsync($"//{route}", true);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}