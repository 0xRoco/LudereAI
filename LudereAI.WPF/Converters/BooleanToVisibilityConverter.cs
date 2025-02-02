using System.Globalization;
using System.Windows.Data;

namespace LudereAI.WPF.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return boolean ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        return System.Windows.Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}