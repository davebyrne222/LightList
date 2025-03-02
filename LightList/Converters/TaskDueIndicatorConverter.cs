using System.Globalization;

namespace LightList.Converters;

public class TaskDueIndicatorConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        
        switch(value) {
            case < 0: return Application.Current.Resources["DueInIndicatorLate"];
            case 0: return Application.Current.Resources["DueInIndicatorDue"];
            case 1: return Application.Current.Resources["DueInIndicatorWarning"];
            default: return Application.Current.Resources["DueInIndicator"];
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}