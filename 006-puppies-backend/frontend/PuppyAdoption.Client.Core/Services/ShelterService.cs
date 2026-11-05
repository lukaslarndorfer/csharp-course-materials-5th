using System.Net;
using System.Net.Http.Json;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Client.Core.Services;

internal sealed class ShelterService(IHttpClientFactory httpClientFactory)
    : ServiceBase(httpClientFactory), IShelterService
{
    public async ValueTask<IReadOnlyList<ShelterDto>> GetAllSheltersAsync()
    {
        await DelayAsync();
        var response = await ApiClient.GetFromJsonAsync<ShelterListResponse>("shelters", JsonOptions);

        return response is null
            ? throw new InvalidOperationException("API returned an invalid format when loading shelters")
            : response.Shelters;
    }

    public async ValueTask<OneOf<Success<ShelterDto>, IShelterService.NameAlreadyExists>>
        CreateNewShelterAsync(string name)
    {
        await DelayAsync();
        using var response
            = await ApiClient.PostAsJsonAsync("shelters", new CreateShelterRequest(name), JsonOptions);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new IShelterService.NameAlreadyExists();
        }

        response.EnsureSuccessStatusCode();
        var shelter = await response.Content.ReadFromJsonAsync<ShelterDto>(JsonOptions);

        return shelter is null
            ? throw new InvalidOperationException("API returned an invalid format when trying to read created Shelter")
            : new Success<ShelterDto>(shelter);
    }
}
