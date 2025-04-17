using System.Globalization;

namespace LightList.Converters;

public class TaskDueTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value as bool? == true)
            return Application.Current!.Resources["TaskItemDescriptionLblDone"];

        return Application.Current!.Resources["TaskItemDescriptionLbl"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}