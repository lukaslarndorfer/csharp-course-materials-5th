using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace PuppyAdoption.Backend.Services.Util;

public static class ServicesSetup
{
    extension(IServiceCollection services)
    {
        public void ConfigureCore()
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddScoped<IShelterService, ShelterService>();
            services.AddScoped<IPuppyService, PuppyService>();
        }
    }
}