using System.Globalization;
using LightList.Utils;
using LightList.ViewModels;

namespace LightList.Converters;

public class TaskDueIndicatorConverter: IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] == null || values[1] == null)
            return Application.Current.Resources["DueInIndicator"];

        if ((bool)values[0] == true)
            return Application.Current.Resources["DueInIndicatorDone"];    

        switch(values[1]) {
            case < 0: return Application.Current.Resources["DueInIndicatorLate"];
            case 0: return Application.Current.Resources["DueInIndicatorDue"];
            case 1: return Application.Current.Resources["DueInIndicatorWarning"];
            default: return Application.Current.Resources["DueInIndicator"];
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}