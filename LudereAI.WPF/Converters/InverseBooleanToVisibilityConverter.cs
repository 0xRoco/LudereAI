﻿using System.Globalization;
using System.Windows.Data;

namespace LudereAI.WPF.Converters;

public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolean)
        {
            return boolean ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }

        return System.Windows.Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}