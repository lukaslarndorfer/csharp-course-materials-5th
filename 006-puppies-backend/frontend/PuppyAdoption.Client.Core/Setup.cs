using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppyAdoption.Client.Shared;

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

        return services;
    }
}
