using System.Globalization;

namespace LightList.Converters;

public class TaskDueLblConverter: IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {

        if (values.Length != 2 || values[0] is null || values[1] is null)
            return Application.Current!.Resources["DueInIndicatorLbl"];

        if ((bool)values[0] == true)
            return Application.Current!.Resources["DueInIndicatorLblDone"];
        
        switch(values[1]) {
            case < 0: return Application.Current!.Resources["DueInIndicatorLblLate"];
            case 0: return Application.Current!.Resources["DueInIndicatorLblDue"];
            case 1: return Application.Current!.Resources["DueInIndicatorLblWarning"];
            default: return Application.Current!.Resources["DueInIndicatorLbl"];
        }
    }

    public object[] ConvertBack(object? value, Type[] targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}