namespace PuppyAdoption.Backend.Persistence.Model;

/// <summary>
///     Read model with basic information about a puppy (projection used by the repository)
/// </summary>
/// <param name="Id">The unique ID of the puppy</param>
/// <param name="Name">The name of the puppy</param>
/// <param name="Adopted">A flag indicating if the puppy has already been adopted</param>
public sealed record PuppyInformation(int Id, string Name, bool Adopted);
