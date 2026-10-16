using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;

namespace PuppyAdoption.Core.Services;

/// <summary>
///     A service for managing shelters
/// </summary>
public interface IShelterService
{
    /// <summary>
    ///     Creates a new shelter with the given name.
    ///     Shelter names must be unique.
    /// </summary>
    /// <param name="name">Name of the shelter</param>
    /// <returns>The created shelter if successful, or an indication that the name is already taken</returns>
    public ValueTask<OneOf<Success<Shelter>, NameAlreadyExists>> CreateNewShelterAsync(string name);

    /// <summary>
    ///     Attempts to retrieve a shelter by its ID
    /// </summary>
    /// <param name="id">The unique ID of the shelter</param>
    /// <returns>The shelter, if found</returns>
    public ValueTask<OneOf<Shelter, NotFound>> GetShelterByIdAsync(int id);

    /// <summary>
    ///     Retrieves all shelters
    /// </summary>
    /// <returns>A collection of all shelters</returns>
    public ValueTask<IReadOnlyCollection<Shelter>> GetAllSheltersAsync();

    /// <summary>
    ///     A result indicating that a shelter with the given name already exists
    /// </summary>
    public readonly record struct NameAlreadyExists;
}

internal sealed class ShelterService(IUnitOfWork uow, ILogger<ShelterService> logger) : ServiceBase, IShelterService
{
    public async ValueTask<OneOf<Shelter, NotFound>> GetShelterByIdAsync(int id)
    {
        var shelter = await uow.ShelterRepository.GetByIdAsync(id);
        if (shelter is null)
        {
            logger.LogWarning("Shelter with ID {ShelterId} not found", id);

            return new NotFound();
        }

        await DelayAsync();

        return shelter;
    }

    public async ValueTask<IReadOnlyCollection<Shelter>> GetAllSheltersAsync()
    {
        IReadOnlyCollection<Shelter> shelters = await uow.ShelterRepository.GetAllAsync();

        await DelayAsync();

        return shelters;
    }

    public async ValueTask<OneOf<Success<Shelter>, IShelterService.NameAlreadyExists>>
        CreateNewShelterAsync(string name)
    {
        if (await uow.ShelterRepository.CheckNameExistsAsync(name))
        {
            logger.LogWarning("Shelter with name {ShelterName} already exists", name);

            return new IShelterService.NameAlreadyExists();
        }

        var shelter = new Shelter
        {
            Name = name,
            Puppies = []
        };
        uow.ShelterRepository.Add(shelter);
        await uow.SaveChangesAsync();

        logger.LogInformation("Created new shelter with ID {ShelterId} and name {ShelterName}", shelter.Id,
                              shelter.Name);

        await DelayAsync();

        return new Success<Shelter>(shelter);
    }
}
