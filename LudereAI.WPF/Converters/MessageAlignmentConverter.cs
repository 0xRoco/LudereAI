using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LudereAI.Shared.Enums;

namespace LudereAI.WPF.Converters;

public class MessageAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageRole role)
        {
            return role switch
            {
                MessageRole.User => HorizontalAlignment.Right,
                MessageRole.Assistant => HorizontalAlignment.Left,
                MessageRole.System => HorizontalAlignment.Center,
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