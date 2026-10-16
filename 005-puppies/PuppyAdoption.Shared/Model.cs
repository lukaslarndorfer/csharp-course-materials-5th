namespace PuppyAdoption.Shared;

/// <summary>
///     Represents basic information about a puppy
/// </summary>
/// <param name="Id">The unique ID of the puppy</param>
/// <param name="Name">The name of the puppy</param>
/// <param name="Adopted">A flag indicating if the puppy has already been adopted</param>
public sealed record PuppyInformation(int Id, string Name, bool Adopted);
