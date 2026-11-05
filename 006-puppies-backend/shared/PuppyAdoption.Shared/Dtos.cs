using NodaTime;

namespace PuppyAdoption.Shared;

public sealed record PuppyInformationDto(int Id, string Name, bool Adopted);

public sealed record PuppyInformationResponse(IReadOnlyList<PuppyInformationDto> Puppies);

public sealed record PuppyDto(
    int Id,
    string Name,
    string Breed,
    LocalDate DateOfBirth,
    LocalDate DateArrived,
    LocalDate? DateAdopted,
    string? AdoptedBy,
    byte[]? ImageData);

public sealed record AdoptPuppyResponse(LocalDate DateAdopted);

public sealed record ShelterDto(int Id, string Name);
