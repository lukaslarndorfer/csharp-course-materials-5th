using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using PuppyAdoption.Client.Core.Services;
using PuppyAdoption.Client.Shared;
using PuppyAdoption.Shared.Validators;

namespace PuppyAdoption.Client.Core;

public static class Setup
{
    public const string ApiClientName = "PuppyApiClient";

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpClient(ApiClientName, (sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<Settings>>().Value;
            client.BaseAddress = new Uri(settings.ApiBaseUrl);
        });

        services.AddSingleton<IShelterService, ShelterService>();
        services.AddSingleton<IPuppyService, PuppyService>();
        services.AddSingleton<AdoptPuppyRequestValidator>();
        services.AddSingleton<CreateShelterRequestValidator>();
        services.AddSingleton(sp => new AdmitPuppyRequestValidator(
                                  sp.GetRequiredService<IClock>(),
                                  sp.GetRequiredService<IOptions<Settings>>().Value.TimeZone));

        return services;
    }
}