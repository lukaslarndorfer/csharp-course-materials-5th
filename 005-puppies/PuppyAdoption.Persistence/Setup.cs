using Microsoft.Extensions.DependencyInjection;

namespace PuppyAdoption.Persistence;

/// <summary>
///     Persistence services setup
/// </summary>
public static class Setup
{
     public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string? connectionString)
     {
          RegisterDatabaseContext(services, connectionString);

          services.AddScoped<UnitOfWork>();
          services.AddScoped<ITransactionProvider>(sp => sp.GetRequiredService<UnitOfWork>());
          services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());

          return services;
     }

     private static void RegisterDatabaseContext(IServiceCollection services, string? connectionString)
     {
          if (string.IsNullOrWhiteSpace(connectionString))
          {
               throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
          }

          var factory = new ContextFactory(true, connectionString);
          services.AddScoped<DatabaseContext>(_ => factory.CreateDbContext([]));
     }
}
