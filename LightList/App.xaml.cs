using System.Globalization;
using CoreGraphics;
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
#if IOS
        CustomiseDatepicker();
        CustomiseEditor();
#endif
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

    /**
     * Create a custom UIView to contain DatePicker and allow selection of time as well as date
     *
     * Problem: Maui uses an accessory view to display the picker and this view is too small
     * causing the time portion of the datepicker to be obscured.
     */
    private void CustomiseDatepicker()
    {
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(IDatePicker), (handler, view) =>
        {
            if (handler?.PlatformView?.InputView is UIDatePicker picker)
            {
                picker.PreferredDatePickerStyle = UIDatePickerStyle.Inline;
            }
        });
    }
    
    private void CustomiseEditor()
    {
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("Border", (handler, view) =>
        {
            if (handler.PlatformView is UITextView uiTextView)
            {
                uiTextView.Layer.BorderColor = CGColor.CreateSrgb(204f/255f, 204f/255f, 204f/255f, 1);
                uiTextView.Layer.BorderWidth = 0.5f;
                uiTextView.Layer.CornerRadius = 5;
            }
        });
    }
}