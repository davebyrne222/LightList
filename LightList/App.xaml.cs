using System.Globalization;
using Foundation;
using LightList.Data;
using LightList.Services;
using LightList.Utils;
using Microsoft.Maui.Platform;
using UIKit;

namespace LightList;

public partial class App : Application
{
    private AppShell _shell;
    public App(AppShell shell)
    {
        Logger.Log("App initializing");
        InitializeComponent();
        _shell = shell;
        CustomiseDatepicker();
        Logger.Log("App initialized");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // TODO: remove / make this work properly
        if (Current != null)
            Current.UserAppTheme = AppTheme.Light;
        
        var culture = new CultureInfo("en-IE");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        return new Window(_shell);
    }

    private void CustomiseDatepicker()
    {
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.PrependToMapping(nameof(IDatePicker), (handler, view) =>
        {
#if IOS || MACCATALYST
            if (handler.PlatformView.InputView is UIDatePicker picker)
            {
                picker.PreferredDatePickerStyle = UIDatePickerStyle.Inline;
                picker.Mode = UIDatePickerMode.DateAndTime; // TODO: doesn't work. Why?
                picker.Frame = new CoreGraphics.CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            }
#endif
        });
    }
    
}