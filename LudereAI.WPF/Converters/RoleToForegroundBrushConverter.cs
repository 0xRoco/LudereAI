using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LudereAI.WPF.Converters;

public sealed class RoleToForegroundBrushConverter : IValueConverter
{
    public Brush AssistantBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDEDED"));
    public Brush UserBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDEDED"));
    public Brush SystemBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3E3E"));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var role = value?.ToString()?.ToLowerInvariant();
        return role switch
        {
            "assistant" => AssistantBrush,
            "user" => UserBrush,
            "system" => SystemBrush,
            _ => AssistantBrush
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

}