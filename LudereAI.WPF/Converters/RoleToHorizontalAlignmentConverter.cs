using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;

namespace LudereAI.WPF.Converters;

public sealed class RoleToHorizontalAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var role = value?.ToString()?.ToLowerInvariant();
        switch (role)
        {
            case "user":
                return System.Windows.HorizontalAlignment.Right;
            case "assistant":
                return System.Windows.HorizontalAlignment.Left;
            case "system":
                return System.Windows.HorizontalAlignment.Center;
            default:
                return System.Windows.HorizontalAlignment.Left;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}