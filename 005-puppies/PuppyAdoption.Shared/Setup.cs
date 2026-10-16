using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace PuppyAdoption.Shared;

/// <summary>
///     Shared services setup
/// </summary>
public static class Setup
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
