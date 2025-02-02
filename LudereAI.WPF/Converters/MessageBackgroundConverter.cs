using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LudereAI.Shared.Enums;

namespace LudereAI.WPF.Converters;

public class MessageBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageRole role)
        {
            return role switch
            {
                MessageRole.User => new SolidColorBrush(Colors.DodgerBlue) ,
                MessageRole.Assistant => new SolidColorBrush(Colors.MidnightBlue),
                MessageRole.System => new SolidColorBrush(Colors.DarkSlateGray),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}