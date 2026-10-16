using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NodaTime;
using NodaTime.Text;

namespace PuppyAdoption.Util;

/// <summary>
///     Allows to format a <see cref="LocalDate"/> as a string in the format "dd.MM.yyyy" for display in the UI.
///     Reverse conversion is not supported.
/// </summary>
public sealed class LocalDateStringConverter : IValueConverter
{
    private static readonly LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("dd.MM.yyyy");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not LocalDate date
            ? string.Empty
            : date == default(LocalDate)
                ? string.Empty
                : pattern.Format(date);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
