using NodaTime;

namespace PuppyAdoption.Core.Services;

/// <summary>
///     Base class for all services in the application
/// </summary>
internal abstract class ServiceBase
{
    private static readonly Duration artificialDelay = Duration.FromSeconds(1.2D);

    /// <summary>
    ///     Adds an artificial delay to simulate network or database latency
    /// </summary>
    protected static async ValueTask DelayAsync()
    {
        await Task.Delay((int) artificialDelay.TotalMilliseconds);
    }
}
