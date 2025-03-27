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

    /**
     * Create a custom UIView to contain DatePicker and allow selection of time as well as date
     *
     * Problem: Maui uses an accessory view to display the picker and this view is too small
     * causing the time portion of the datepicker to be obscured.
     */
    private void CustomiseDatepicker()
    {
#if IOS
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.ModifyMapping(nameof(IDatePicker), (handler, view, action) =>
        {
            var datePicker = new UIDatePicker
            {
                Mode = UIDatePickerMode.DateAndTime,
                PreferredDatePickerStyle = UIDatePickerStyle.Inline,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            var containerView = new UIView();
            
            containerView.AddSubview(datePicker);

            // Set the frame or constraints for proper sizing
            containerView.Frame = new CoreGraphics.CGRect(0, 0, handler.PlatformView.Frame.Width, 500);

            // Set constraints for better layout
            datePicker.TopAnchor.ConstraintEqualTo(containerView.TopAnchor).Active = true;
            datePicker.LeadingAnchor.ConstraintEqualTo(containerView.LeadingAnchor).Active = true;
            datePicker.TrailingAnchor.ConstraintEqualTo(containerView.TrailingAnchor).Active = true;
            datePicker.BottomAnchor.ConstraintEqualTo(containerView.BottomAnchor).Active = true;

            // Assign the custom input view
            handler.PlatformView.InputView = containerView; 
        });
#endif
    }
    
}