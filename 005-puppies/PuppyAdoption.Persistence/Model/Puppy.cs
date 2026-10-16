using NodaTime;

namespace PuppyAdoption.Persistence.Model;

/// <summary>
///     Represents a puppy entity
/// </summary>
public class Puppy
{
    /// <summary>
    ///     The unique identifier for the puppy
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     The name of the puppy
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     The breed of the puppy
    /// </summary>
    public required string Breed { get; set; }

    /// <summary>
    ///     The date of birth of the puppy
    /// </summary>
    public LocalDate DateOfBirth { get; set; }

    /// <summary>
    ///     The date the puppy arrived at the shelter
    /// </summary>
    public LocalDate DateArrived { get; set; }

    /// <summary>
    ///     The date the puppy was adopted, if applicable
    /// </summary>
    public LocalDate? DateAdopted { get; set; }

    /// <summary>
    ///     The name of the person who adopted the puppy, if applicable
    /// </summary>
    public string? AdoptedBy { get; set; }

    /// <summary>
    ///     The ID of the shelter where the puppy is (was) located
    /// </summary>
    public int ShelterId { get; set; }

    /// <summary>
    ///     Optional image data of the puppy
    /// </summary>
    public byte[]? Image { get; set; }

    /// <summary>
    ///     Reference to the shelter where the puppy is (was) located
    /// </summary>
    public required Shelter Shelter { get; set; }
}
