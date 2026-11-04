using Microsoft.Extensions.DependencyInjection;
using PuppyAdoption.Client.Core.Services;

namespace PuppyAdoption.Client.Core;

/// <summary>
///     Core services setup
/// </summary>
public static class Setup
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IShelterService, ShelterService>();
        services.AddScoped<IPuppyService, PuppyService>();

        return services;
    }
}