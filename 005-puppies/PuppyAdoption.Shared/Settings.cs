using NodaTime;

namespace PuppyAdoption.Shared;

/// <summary>
///     Application settings
/// </summary>
public class Settings
{
    public const string SectionKey = "General";

    /// <summary>
    ///     Defines the maximum width and height of images in pixels
    /// </summary>
    public int MaxImageSizePixel { get; set; }

    /// <summary>
    ///     Time zone ID for the application
    /// </summary>
    public required string TimeZoneId { get; set; }

    /// <summary>
    ///     Time zone for the application, based on the <see cref="TimeZoneId"/>
    /// </summary>
    public DateTimeZone TimeZone => DateTimeZoneProviders.Tzdb[TimeZoneId];
}
