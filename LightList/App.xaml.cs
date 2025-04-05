using System.Globalization;
using CoreGraphics;
using LightList.Utils;
using Microsoft.Maui.Handlers;
using UIKit;

namespace LightList;

public partial class App : Application
{
    private readonly ILogger _logger;
    private readonly AppShell _shell;

    public App(ILogger logger, AppShell shell)
    {
        _logger = logger;
        _logger.Debug("App initializing");
        InitializeComponent();
        _shell = shell;
#if IOS
        CustomiseDatepicker();
        CustomiseEditor();
#endif
        _logger.Debug("App initialized");
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
        DatePickerHandler.Mapper.AppendToMapping(nameof(IDatePicker), (handler, view) =>
        {
            if (handler?.PlatformView?.InputView is UIDatePicker picker)
                picker.PreferredDatePickerStyle = UIDatePickerStyle.Inline;
        });
    }

    private void CustomiseEditor()
    {
        EditorHandler.Mapper.AppendToMapping("Border", (handler, view) =>
        {
            if (handler.PlatformView is not UITextView uiTextView)
                return;

            // Default fallback color (Gray100: #CCCCCC)
            (float red, float green, float blue, float alpha) borderColor = (204f / 255f, 204f / 255f, 204f / 255f, 1f);

            // Attempt to get the color from resources
            if (Current?.Resources.TryGetValue("BorderBrush", out var colorValue) == true
                && colorValue is SolidColorBrush borderBrush)
                borderColor = (
                    borderBrush.Color.Red,
                    borderBrush.Color.Green,
                    borderBrush.Color.Blue,
                    borderBrush.Color.Alpha);

            // Apply border properties
            uiTextView.Layer.BorderColor = CGColor.CreateSrgb(
                borderColor.red,
                borderColor.green,
                borderColor.blue,
                borderColor.alpha);
            uiTextView.Layer.BorderWidth = 0.5f;
            uiTextView.Layer.CornerRadius = 5;
        });
    }
}