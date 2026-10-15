using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PiCalc.Util;

/// <summary>
///     Converts an <see cref="int" /> to a <see cref="double" /> and vice versa
/// </summary>
internal sealed class IntToDoubleConverter : IValueConverter
{
    /// <summary>
    ///     Converts an <see cref="int" /> to a <see cref="double" />.
    ///     If the value is not an <see cref="int" />, it returns 0D.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <param name="targetType">Unused</param>
    /// <param name="parameter">Unused</param>
    /// <param name="culture">Unused</param>
    /// <returns>The converted or a fallback value </returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return (double) intValue;
        }

        return 0D;
    }

    /// <summary>
    ///     Converts a <see cref="double" /> back to an <see cref="int" />.
    ///     If the value is not a <see cref="double" />, it returns 0.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <param name="targetType">Unused</param>
    /// <param name="parameter">Unused</param>
    /// <param name="culture">Unused</param>
    /// <returns>The converted or a fallback value </returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return (int) doubleValue;
        }

        return 0;
    }
}
