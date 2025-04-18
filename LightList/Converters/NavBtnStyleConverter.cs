using System.Globalization;

namespace LightList.Converters;

public class NavBtnStyleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Application.Current!.Resources["NavBarButtonStyle"];

        return value.ToString() == parameter.ToString()
            ? Application.Current!.Resources["SelectedNavBarButtonStyle"]
            : Application.Current!.Resources["NavBarButtonStyle"];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}