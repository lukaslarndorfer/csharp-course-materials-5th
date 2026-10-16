namespace PuppyAdoption.Persistence.Model;

/// <summary>
///     Represents a shelter entity
/// </summary>
public class Shelter
{
    /// <summary>
    ///     The unique identifier for the shelter
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     The unique name of the shelter
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     All puppies which are (were) located in this shelter
    /// </summary>
    public required List<Puppy> Puppies { get; set; } = [];
}
