using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace LightList.ViewModels;

public class NavBarViewModel: INotifyPropertyChanged
{
    private string _selectedView = "All";
    
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

    public NavBarViewModel()
    {
        OpenMenuCommand = new Command(OpenMenu);
        AddTaskCommand = new Command(AddTask);
        NavigateCommand = new Command<string>(NavigateToPage);
    }

    private static void OpenMenu()
    {
        Debug.WriteLine("---[NavBarViewModel.OpenMenu] Opening Flyout");
        Shell.Current.FlyoutIsPresented = true;
    }

    private async void AddTask()
    {
        if (Shell.Current.CurrentPage is Views.TaskPage)
            return;
        
        Debug.WriteLine($"---[NavBarViewModel.AddTask] Navigating to {nameof(Views.TaskPage)}");
        await Shell.Current.GoToAsync(nameof(Views.TaskPage));
    }
    
    private async void NavigateToPage(string pageName)
    {
        string? route = pageName switch
        {
            "All" => nameof(Views.AllTasksPage),
            "Due" => nameof(Views.TasksByDueDatePage),
            "Label" => nameof(Views.TasksByLabelPage),
            _ => null
        };
        
        if (route == null || Shell.Current.CurrentPage?.GetType().Name == route)
            return;

        // Update styling
        SelectedView = pageName;

        Debug.WriteLine($"---[NavBarViewModel.NavigateToPage] Navigating to {route}");
        await Shell.Current.GoToAsync($"//{route}", false);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}