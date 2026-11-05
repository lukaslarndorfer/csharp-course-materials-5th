namespace PuppyAdoption.Shared;

public sealed record PuppyInformationDto(int Id, string Name, bool Adopted);

public sealed record PuppyInformationResponse(IReadOnlyList<PuppyInformationDto> Puppies);

public sealed record ShelterDto(int Id, string Name);
