using Microsoft.Extensions.Options;
using NodaTime;

namespace PuppyAdoption.Shared;

/// <summary>
///     Provides methods to access the current date and time in a specific time zone
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    ///     Gets the current instant in time, which is the same across all time zones
    /// </summary>
    /// <returns>Current instant</returns>
    public Instant GetCurrentInstant();

    /// <summary>
    ///     Gets the current local date and time in the configured time zone
    /// </summary>
    /// <returns>Local date and time</returns>
    public LocalDateTime GetLocalDateTime();

    /// <summary>
    ///     Gets the current local time in the configured time zone
    /// </summary>
    /// <returns>Local time</returns>
    public LocalTime GetLocalTime();

    /// <summary>
    ///     Gets the current local date in the configured time zone
    /// </summary>
    /// <returns>Local date</returns>
    public LocalDate GetLocalDate();

    /// <summary>
    ///     Puts the given local date and time into the configured time zone and returns the corresponding instant
    /// </summary>
    /// <param name="dateTime">The date and time to use</param>
    /// <returns>The instant representation of the local date and time</returns>
    public Instant ToInstantInZone(LocalDateTime dateTime);

    /// <summary>
    ///     Puts the given local date and optional time into the configured time zone and returns the corresponding instant
    /// </summary>
    /// <param name="date">The date to use</param>
    /// <param name="atTime">The optional time to use - if omitted midnight is assumed</param>
    /// <returns>The instant representation of the local date and optional time</returns>
    public Instant ToInstantInZone(LocalDate date, LocalTime? atTime = null);
}

internal sealed class DateTimeProvider(IOptions<Settings> options, IClock clock) : IDateTimeProvider
{
    private readonly DateTimeZone _timeZone = options.Value.TimeZone;

    public Instant GetCurrentInstant() => clock.GetCurrentInstant();
    public LocalDateTime GetLocalDateTime() => GetLocalNow().LocalDateTime;
    public LocalTime GetLocalTime() => GetLocalDateTime().TimeOfDay;
    public LocalDate GetLocalDate() => GetLocalDateTime().Date;

    public Instant ToInstantInZone(LocalDateTime dateTime) => ToInstantInZone(dateTime.Date, dateTime.TimeOfDay);

    public Instant ToInstantInZone(LocalDate date, LocalTime? atTime = null)
    {
        var midnight = date.AtStartOfDayInZone(_timeZone);
        var effectiveZonedDateTime = atTime.HasValue
            ? midnight.Date.At(atTime.Value).InZoneLeniently(_timeZone)
            : midnight;

        return effectiveZonedDateTime.ToInstant();
    }

    private ZonedDateTime GetLocalNow() => clock.GetCurrentInstant().InZone(_timeZone);
}
