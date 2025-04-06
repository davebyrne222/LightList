using LightList.ViewModels;

namespace LightList.Views.Components;

public partial class NavBar : ContentView
{
    public NavBar() : this(MauiProgram.GetService<NavBarViewModel>() ?? throw new InvalidOperationException())
    {
    }

    public NavBar(NavBarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}