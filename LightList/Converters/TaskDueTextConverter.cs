using System.Globalization;
using LightList.Utils;

namespace LightList.Converters;

public class TaskDueTextConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value as bool? == true)
            return Application.Current.Resources["TaskLabelDone"];

        return Application.Current.Resources["BaseLabel"];
        
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}