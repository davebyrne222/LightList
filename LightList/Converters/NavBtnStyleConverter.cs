using System.Globalization;

namespace LightList.Converters;

public class NavBtnStyleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Application.Current.Resources["FilterButtonStyle"];

        return value.ToString() == parameter.ToString()
            ? Application.Current.Resources["SelectedFilterButtonStyle"]
            : Application.Current.Resources["FilterButtonStyle"];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}