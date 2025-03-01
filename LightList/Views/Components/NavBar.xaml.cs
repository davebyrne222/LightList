using LightList.ViewModels;

namespace LightList.Views.Components;

public partial class NavBar : ContentView
{
    public NavBar() : this(MauiProgram.GetService<NavBarViewModel>())
    {
    }

    public NavBar(NavBarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}