using LightList.ViewModels;

namespace LightList.Views.Components;

public partial class NavBar : ContentView
{
    public NavBar()
    {
        InitializeComponent();
        BindingContext = new NavBarViewModel();
    }
}