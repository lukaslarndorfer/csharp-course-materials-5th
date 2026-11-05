using System.Net;
using PuppyAdoption.Backend.Persistence.Model;
using PuppyAdoption.Backend.Persistence.Util;
using PuppyAdoption.Backend.TestInt.Util;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Backend.TestInt;

public sealed class ShelterTests(WebApiTestFixture webApiFixture) : WebApiTestBase(webApiFixture)
{
    private const string BaseUrl = "api/shelters";

    private int _shelterId;

    protected override async ValueTask ImportSeedDataAsync(DatabaseContext context)
    {
        await base.ImportSeedDataAsync(context);

        var shelter1 = new Shelter
        {
            Name = "Shelter 1",
            Puppies = []
        };
        var shelter2 = new Shelter
        {
            Name = "Shelter 2",
            Puppies = []
        };

        context.Shelters.Add(shelter1);
        context.Shelters.Add(shelter2);
        await context.SaveChangesAsync();

        _shelterId = shelter1.Id;
    }

    [Fact]
    public async ValueTask GetAllShelters_Success()
    {
        var response = await ApiClient.GetAsync(BaseUrl, TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ShelterListResponse>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Shelters.Should().HaveCount(2);
        content.Shelters.Select(s => s.Name).Should().BeEquivalentTo(["Shelter 1", "Shelter 2"]);
    }

    [Fact]
    public async ValueTask GetShelterById_Success()
    {
        var response = await ApiClient.GetAsync($"{BaseUrl}/{_shelterId}", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<ShelterDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Id.Should().Be(_shelterId);
        content.Name.Should().Be("Shelter 1");
    }

    [Fact]
    public async ValueTask GetShelterById_NotFound()
    {
        var response = await ApiClient.GetAsync($"{BaseUrl}/9999", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async ValueTask CreateShelter_Success()
    {
        var response = await ApiClient.PostAsJsonAsync(BaseUrl, new CreateShelterRequest("Shelter 3"), JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<ShelterDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Name.Should().Be("Shelter 3");
        response.Headers.Location.Should().NotBeNull()
                .And.Match<Uri>(uri => uri.AbsoluteUri.EndsWith($"{BaseUrl}/{content.Id}"));

        // the shelter has to be persisted, which only happens if the transaction was committed
        var loaded = await ApiClient.GetAsync($"{BaseUrl}/{content.Id}", TestCancellationToken);
        loaded.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async ValueTask CreateShelter_NameAlreadyExists()
    {
        var response = await ApiClient.PostAsJsonAsync(BaseUrl, new CreateShelterRequest("Shelter 1"), JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async ValueTask CreateShelter_EmptyName()
    {
        var response = await ApiClient.PostAsJsonAsync(BaseUrl, new CreateShelterRequest(string.Empty), JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}