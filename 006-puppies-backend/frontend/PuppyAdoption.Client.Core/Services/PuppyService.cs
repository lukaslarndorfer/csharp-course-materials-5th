using System.Net;
using System.Net.Http.Json;
using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Client.Core.Services;

internal sealed class PuppyService(IHttpClientFactory httpClientFactory) : ServiceBase(httpClientFactory), IPuppyService
{
    public async ValueTask<OneOf<Success<PuppyInformationResponse>, NotFound>> GetAllPuppiesAsync(
        int shelterId)
    {
        await DelayAsync();
        using var response = await ApiClient.GetAsync($"shelters/{shelterId}/puppies");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFound();
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<PuppyInformationResponse>(JsonOptions);

        return content is null
            ? throw new InvalidOperationException("API returned an invalid format when trying to get all puppies")
            : new Success<PuppyInformationResponse>(content);
    }

    public async ValueTask<OneOf<PuppyDto, NotFound>> GetPuppyByIdAsync(int puppyId)
    {
        await DelayAsync();
        using var response = await ApiClient.GetAsync($"puppies/{puppyId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFound();
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<PuppyDto>(JsonOptions);

        return content ??
               throw new
                   InvalidOperationException($"API returned an invalid format when trying to get puppy by id: {puppyId}");
    }

    public async ValueTask<OneOf<Success<PuppyDto>, NotFound>> AdmitPuppyAsync(
        int shelterId, string name, string breed, LocalDate dateOfBirth, byte[]? imageData)
    {
        await DelayAsync();
        using var response = await ApiClient.PostAsJsonAsync($"shelters/{shelterId}/puppies",
                                                             new AdmitPuppyRequest(name, breed, dateOfBirth, imageData),
                                                             JsonOptions);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFound();
        }

        response.EnsureSuccessStatusCode();
        var puppy = await response.Content.ReadFromJsonAsync<PuppyDto>(JsonOptions);

        return puppy is null
            ? throw new InvalidOperationException("API returned an invalid format when trying to read created puppy")
            : new Success<PuppyDto>(puppy);
    }

    public async ValueTask<OneOf<Success<LocalDate>, NotFound, IPuppyService.AlreadyAdopted>>
        AdoptPuppyAsync(int puppyId, string adopterName)
    {
        await DelayAsync();
        using var response = await ApiClient.PostAsJsonAsync($"puppies/{puppyId}/adoption",
                                                             new AdoptPuppyRequest(adopterName),
                                                             JsonOptions);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFound();
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return new IPuppyService.AlreadyAdopted();
        }

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AdoptPuppyResponse>(JsonOptions);

        return content is null
            ? throw new InvalidOperationException("API returned an invalid format when trying to adopt puppy")
            : new Success<LocalDate>(content.DateAdopted);
    }

    public async ValueTask<OneOf<Success, NotFound>> DeletePuppyAsync(int puppyId)
    {
        await DelayAsync();
        using var response = await ApiClient.DeleteAsync($"puppies/{puppyId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new NotFound();
        }

        response.EnsureSuccessStatusCode();

        return new Success();
    }
}