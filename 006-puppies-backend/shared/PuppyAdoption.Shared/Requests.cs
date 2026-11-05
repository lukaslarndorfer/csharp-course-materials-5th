using NodaTime;

namespace PuppyAdoption.Shared;

public sealed record AdmitPuppyRequest(string Name, string Breed, LocalDate DateOfBirth, byte[]? ImageData);

