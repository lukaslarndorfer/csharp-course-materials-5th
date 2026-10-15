using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace PiCalc.Core;

public static class CoreSetup
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);

        services.AddTransient<IRandomProvider, RandomProvider>();

        services.AddScoped<IRunner, Runner>();

        services.AddKeyedTransient<IPiCalculation, PiCalculation>(IPiCalculation.BasicCalculation);
        services.AddKeyedTransient<IPiCalculation, PiCalculationSIMD>(IPiCalculation.VectorCalculation);
    }
}
