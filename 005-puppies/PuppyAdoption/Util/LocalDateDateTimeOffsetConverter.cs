using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NodaTime;

namespace PuppyAdoption.Util;

/// <summary>
///     Converts between <see cref="LocalDate"/> and <see cref="DateTimeOffset"/>
/// </summary>
public sealed class LocalDateDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is LocalDate localDate
            ? new DateTimeOffset(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0, TimeSpan.Zero)
            : new DateTimeOffset();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTimeOffset dateTimeOffset
            ? new LocalDate(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day)
            : LocalDate.FromDateTime(DateTime.Today);
}
