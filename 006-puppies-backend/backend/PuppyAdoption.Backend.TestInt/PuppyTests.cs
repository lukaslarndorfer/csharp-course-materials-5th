using System.Net;
using PuppyAdoption.Backend.Persistence.Model;
using PuppyAdoption.Backend.Persistence.Util;
using PuppyAdoption.Backend.TestInt.Util;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Backend.TestInt;

public sealed class PuppyTests(WebApiTestFixture webApiFixture) : WebApiTestBase(webApiFixture)
{
    private const string ShelterUrl = "api/shelters";
    private const string PuppyUrl = "api/puppies";

    // the test clock is fixed to this date
    private static readonly LocalDate Today = new(2026, 01, 01);

    private int _shelterId;
    private int _emptyShelterId;
    private int _availablePuppyId;
    private int _adoptedPuppyId;

    protected override async ValueTask ImportSeedDataAsync(DatabaseContext context)
    {
        await base.ImportSeedDataAsync(context);

        var shelter = new Shelter
        {
            Name = "Shelter 1",
            Puppies = []
        };
        var emptyShelter = new Shelter
        {
            Name = "Shelter 2",
            Puppies = []
        };

        var availablePuppy = new Puppy
        {
            Name = "Bello",
            Breed = "White Dog",
            DateOfBirth = new LocalDate(2025, 06, 01),
            DateArrived = new LocalDate(2025, 09, 01),
            Shelter = shelter
        };
        var adoptedPuppy = new Puppy
        {
            Name = "Luna",
            Breed = "Black Dog",
            DateOfBirth = new LocalDate(2025, 03, 15),
            DateArrived = new LocalDate(2025, 05, 01),
            DateAdopted = new LocalDate(2025, 11, 20),
            AdoptedBy = "John Doe",
            Shelter = shelter
        };

        context.Shelters.Add(shelter);
        context.Shelters.Add(emptyShelter);
        context.Puppies.Add(availablePuppy);
        context.Puppies.Add(adoptedPuppy);
        await context.SaveChangesAsync();

        _shelterId = shelter.Id;
        _emptyShelterId = emptyShelter.Id;
        _availablePuppyId = availablePuppy.Id;
        _adoptedPuppyId = adoptedPuppy.Id;
    }

    [Fact]
    public async ValueTask GetAllPuppies_Success()
    {
        var response = await ApiClient.GetAsync($"{ShelterUrl}/{_shelterId}/puppies", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content
            = await response.Content.ReadFromJsonAsync<PuppyInformationResponse>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Puppies.Should().HaveCount(2);
        content.Puppies.Should().ContainSingle(p => p.Id == _availablePuppyId && p.Name == "Bello" && !p.Adopted);
        content.Puppies.Should().ContainSingle(p => p.Id == _adoptedPuppyId && p.Name == "Luna" && p.Adopted);
    }

    [Fact]
    public async ValueTask GetAllPuppies_ShelterNotFound()
    {
        var response = await ApiClient.GetAsync($"{ShelterUrl}/9999/puppies", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async ValueTask GetPuppyById_Success()
    {
        var response = await ApiClient.GetAsync($"{PuppyUrl}/{_adoptedPuppyId}", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<PuppyDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Id.Should().Be(_adoptedPuppyId);
        content.Name.Should().Be("Luna");
        content.Breed.Should().Be("Black Dog");
        content.DateOfBirth.Should().Be(new LocalDate(2025, 03, 15));
        content.DateArrived.Should().Be(new LocalDate(2025, 05, 01));
        content.DateAdopted.Should().Be(new LocalDate(2025, 11, 20));
        content.AdoptedBy.Should().Be("John Doe");
        content.ImageData.Should().BeNull();
    }

    [Fact]
    public async ValueTask GetPuppyById_NotFound()
    {
        var response = await ApiClient.GetAsync($"{PuppyUrl}/9999", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async ValueTask GetPuppyById_InvalidId()
    {
        var response = await ApiClient.GetAsync($"{PuppyUrl}/0", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask AdmitPuppy_Success()
    {
        var request = new AdmitPuppyRequest("Max", "Brown Dog", new LocalDate(2025, 12, 01), null);

        var response = await ApiClient.PostAsJsonAsync($"{ShelterUrl}/{_emptyShelterId}/puppies", request, JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<PuppyDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.Name.Should().Be("Max");
        content.Breed.Should().Be("Brown Dog");
        content.DateOfBirth.Should().Be(new LocalDate(2025, 12, 01));
        content.DateArrived.Should().Be(Today);
        content.DateAdopted.Should().BeNull();
        content.AdoptedBy.Should().BeNull();
        response.Headers.Location.Should().NotBeNull()
                .And.Match<Uri>(uri => uri.AbsoluteUri.EndsWith($"{PuppyUrl}/{content.Id}"));


        var puppies = await ApiClient.GetFromJsonAsync<PuppyInformationResponse>($"{ShelterUrl}/{_emptyShelterId}/puppies",
                                                                                 JsonOptions, TestCancellationToken);
        puppies.Should().NotBeNull();
        puppies.Puppies.Should().ContainSingle(p => p.Id == content.Id);
    }

    [Fact]
    public async ValueTask AdmitPuppy_WithImage()
    {
        byte[] image = [1, 2, 3, 4, 5];
        var request = new AdmitPuppyRequest("Max", "Brown Dog", Today, image);

        var response = await ApiClient.PostAsJsonAsync($"{ShelterUrl}/{_shelterId}/puppies", request, JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<PuppyDto>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.ImageData.Should().Equal(image);
    }

    [Fact]
    public async ValueTask AdmitPuppy_ShelterNotFound()
    {
        var request = new AdmitPuppyRequest("Max", "Brown Dog", Today, null);

        var response = await ApiClient.PostAsJsonAsync($"{ShelterUrl}/9999/puppies", request, JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async ValueTask AdmitPuppy_EmptyNameAndBreed()
    {
        var request = new AdmitPuppyRequest(string.Empty, string.Empty, Today, null);

        var response = await ApiClient.PostAsJsonAsync($"{ShelterUrl}/{_shelterId}/puppies", request, JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask AdmitPuppy_DateOfBirthInFuture()
    {
        var request = new AdmitPuppyRequest("Max", "Brown Dog", Today.PlusDays(1), null);

        var response = await ApiClient.PostAsJsonAsync($"{ShelterUrl}/{_shelterId}/puppies", request, JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask AdmitPuppy_ImageTooLarge()
    {
        var image = new byte[(1024 * 1024) + 1];
        var request = new AdmitPuppyRequest("Max", "Brown Dog", Today, image);

        var response = await ApiClient.PostAsJsonAsync($"{ShelterUrl}/{_shelterId}/puppies", request, JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask AdoptPuppy_Success()
    {
        var response = await ApiClient.PostAsJsonAsync($"{PuppyUrl}/{_availablePuppyId}/adoption",
                                                       new AdoptPuppyRequest("Jane Doe"), JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<AdoptPuppyResponse>(JsonOptions, TestCancellationToken);

        content.Should().NotBeNull();
        content.DateAdopted.Should().Be(Today);

        var puppy = await ApiClient.GetFromJsonAsync<PuppyDto>($"{PuppyUrl}/{_availablePuppyId}", JsonOptions,
                                                               TestCancellationToken);
        puppy.Should().NotBeNull();
        puppy.DateAdopted.Should().Be(Today);
        puppy.AdoptedBy.Should().Be("Jane Doe");
    }

    [Fact]
    public async ValueTask AdoptPuppy_AlreadyAdopted()
    {
        var response = await ApiClient.PostAsJsonAsync($"{PuppyUrl}/{_adoptedPuppyId}/adoption",
                                                       new AdoptPuppyRequest("Jane Doe"), JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // the original adoption must not be changed
        var puppy = await ApiClient.GetFromJsonAsync<PuppyDto>($"{PuppyUrl}/{_adoptedPuppyId}", JsonOptions,
                                                               TestCancellationToken);
        puppy.Should().NotBeNull();
        puppy.AdoptedBy.Should().Be("John Doe");
        puppy.DateAdopted.Should().Be(new LocalDate(2025, 11, 20));
    }

    [Fact]
    public async ValueTask AdoptPuppy_NotFound()
    {
        var response = await ApiClient.PostAsJsonAsync($"{PuppyUrl}/9999/adoption", new AdoptPuppyRequest("Jane Doe"),
                                                       JsonOptions, TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async ValueTask AdoptPuppy_EmptyAdopterName()
    {
        var response = await ApiClient.PostAsJsonAsync($"{PuppyUrl}/{_availablePuppyId}/adoption",
                                                       new AdoptPuppyRequest(string.Empty), JsonOptions,
                                                       TestCancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask DeletePuppy_Success()
    {
        var response = await ApiClient.DeleteAsync($"{PuppyUrl}/{_availablePuppyId}", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleted = await ApiClient.GetAsync($"{PuppyUrl}/{_availablePuppyId}", TestCancellationToken);
        deleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async ValueTask DeletePuppy_NotFound()
    {
        var response = await ApiClient.DeleteAsync($"{PuppyUrl}/9999", TestCancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
