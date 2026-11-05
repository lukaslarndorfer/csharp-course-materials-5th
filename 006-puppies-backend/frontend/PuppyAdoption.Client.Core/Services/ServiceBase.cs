using System.Text.Json;
using NodaTime;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Client.Core.Services;

/// <summary>
///     Base class for all services in the application
/// </summary>
internal abstract class ServiceBase(IHttpClientFactory httpClientFactory)
{
    private static readonly Duration artificialDelay = Duration.FromSeconds(0.6D);

    private static readonly Lazy<JsonSerializerOptions> jsonOptions = new(() =>
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Web);
        JsonConfig.ConfigureJsonSerialization(options, false);

        return options;
    });

    /// <summary>
    ///     HTTP client used to communicate with the API
    /// </summary>
    protected HttpClient ApiClient { get; } = httpClientFactory.CreateClient(Setup.ApiClientName);

    /// <summary>
    ///     JSON serialization options used for API requests and responses
    /// </summary>
    protected static JsonSerializerOptions JsonOptions => jsonOptions.Value;

    /// <summary>
    ///     Adds an artificial delay to simulate network or database latency
    /// </summary>
    protected static async ValueTask DelayAsync()
    {
        await Task.Delay((int) artificialDelay.TotalMilliseconds);
    }
}