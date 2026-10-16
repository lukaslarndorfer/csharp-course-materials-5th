using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PuppyAdoption.Persistence;

/// <summary>
///     Factory for creating a <see cref="DatabaseContext" />
/// </summary>
/// <param name="connectionStringOverride">Allows to set a different connection string than the default</param>
public sealed class ContextFactory(bool enableConsoleLogging, string? connectionStringOverride = null)
    : IDesignTimeDbContextFactory<DatabaseContext>
{
    private const string DefaultConnectionString
        = "Host=localhost;Database=postgres;Username=postgres;Password=postgres";

    // parameterless ctor required for EF Core tools
    public ContextFactory() : this(true) { }

    /// <inheritdoc />
    public DatabaseContext CreateDbContext(string[] _)
    {
        string connectionString = connectionStringOverride ?? DefaultConnectionString;
        DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                                                                  .UseNpgsql(connectionString,
                                                                             options =>
                                                                             {
                                                                                 options.UseNodaTime();
                                                                                 options
                                                                                     .MigrationsHistoryTable("__EFMigrationsHistory",
                                                                                          DatabaseContext.SchemaName);
                                                                             })
                                                                  .ConfigureWarnings(warnings =>
                                                                           warnings.Throw(RelationalEventId
                                                                                    .MultipleCollectionIncludeWarning));

        if (enableConsoleLogging)
        {
            optionsBuilder.EnableSensitiveDataLogging() // don't use in production
                          .EnableDetailedErrors() // don't use in production
                          .LogTo(Console.WriteLine, LogLevel.Information);
        }

        return new DatabaseContext(optionsBuilder.Options);
    }
}
