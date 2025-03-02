using System.Globalization;

namespace LightList.Converters;

public class TaskDueLblConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        
        switch(value) {
            case < 0: return Application.Current.Resources["DueInIndicatorLblLate"];
            case 0: return Application.Current.Resources["DueInIndicatorLblDue"];
            case 1: return Application.Current.Resources["DueInIndicatorLblWarning"];
            default: return Application.Current.Resources["DueInIndicatorLbl"];
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}