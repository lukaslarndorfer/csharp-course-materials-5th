using OneOf;
using OneOf.Types;
using PuppyAdoption.Backend.Persistence.Model;
using PuppyAdoption.Backend.Persistence.Util;
using PuppyAdoption.Backend.Shared;

namespace PuppyAdoption.Backend.Services;

/// <summary>
///     Service for managing puppies in the shelter
/// </summary>
public interface IPuppyService
{
    /// <summary>
    ///     Gets basic information about all puppies in the given shelter
    /// </summary>
    /// <param name="shelterId">The id of the shelter for which to retrieve puppy data</param>
    /// <returns>Overview information about all puppies in the shelter or <see cref="NotFound"/> if no shelter with the given id does exist</returns>
    public ValueTask<OneOf<Success<IReadOnlyCollection<PuppyInformation>>, NotFound>> GetAllPuppiesAsync(int shelterId);

    /// <summary>
    ///     Admits a new puppy to the shelter with the given details
    /// </summary>
    /// <param name="shelterId">The id of the shelter to which to admit the puppy to</param>
    /// <param name="name">Name of the puppy</param>
    /// <param name="breed">Breed of the puppy</param>
    /// <param name="dateOfBirth">Date of birth of the puppy</param>
    /// <param name="imageData">Optionally, bytes of an image of the puppy</param>
    /// <returns>The created puppy entity or <see cref="NotFound"/> if no shelter with the given id does exist</returns>
    public ValueTask<OneOf<Success<Puppy>, NotFound>> AdmitPuppyAsync(int shelterId, string name, string breed,
                                                                      LocalDate dateOfBirth, byte[]? imageData);

    /// <summary>
    ///     Attempts to retrieve a puppy by its ID.
    ///     Puppy IDs are unique across all shelters.
    /// </summary>
    /// <param name="puppyId">The ID of the puppy</param>
    /// <returns>Either the puppy or a result indicating that it was not found</returns>
    public ValueTask<OneOf<Puppy, NotFound>> GetPuppyByIdAsync(int puppyId);

    /// <summary>
    ///     Attempts to delete a puppy by its ID
    /// </summary>
    /// <param name="puppyId">The ID of the puppy to delete</param>
    /// <returns>A result indicating if the puppy could be deleted or was not found</returns>
    public ValueTask<OneOf<Success, NotFound>> DeletePuppyAsync(int puppyId);

    /// <summary>
    ///     Attempts to have someone adopt a puppy.
    ///     Puppies can only be adopted once.
    /// </summary>
    /// <param name="puppyId">The ID of the puppy to adopt</param>
    /// <param name="adopterName">The name of the person adopting the puppy</param>
    /// <returns>
    ///     A result indicating if the (and when) the puppy was adopted, or if it was not found, or if it was already
    ///     adopted previously
    /// </returns>
    public ValueTask<OneOf<Success<LocalDate>, NotFound, AlreadyAdopted>> AdoptPuppyAsync(
        int puppyId, string adopterName);

    /// <summary>
    ///     A result indicating that a puppy was already adopted and cannot be adopted again
    /// </summary>
    public readonly record struct AlreadyAdopted;
}

internal sealed class PuppyService(IUnitOfWork uow, IClock clock, ILogger<PuppyService> logger)
    : IPuppyService
{
    public async ValueTask<OneOf<Success<IReadOnlyCollection<PuppyInformation>>, NotFound>> GetAllPuppiesAsync(
        int shelterId)
    {
        var shelter = await uow.ShelterRepository.GetByIdAsync(shelterId);
        if (shelter is null)
        {
            logger.LogWarning("Shelter with id {Id} doesnt exist", shelterId);

            return new NotFound();
        }

        IReadOnlyCollection<PuppyInformation> puppies = await uow.PuppyRepository.GetAllAsync(shelter);
        logger.LogDebug("Received {Count} puppies", puppies.Count);

        return new Success<IReadOnlyCollection<PuppyInformation>>(puppies);
    }

    public async ValueTask<OneOf<Success<Puppy>, NotFound>> AdmitPuppyAsync(int shelterId, string name, string breed,
                                                                            LocalDate dateOfBirth, byte[]? imageData)
    {
        var shelter = await uow.ShelterRepository.GetByIdAsync(shelterId);
        if (shelter is null)
        {
            logger.LogWarning("Shelter with id {Id} doesnt exist", shelterId);

            return new NotFound();
        }

        Puppy puppy = new()
        {
            Shelter = shelter,
            ShelterId = shelterId,
            Name = name,
            Breed = breed,
            DateOfBirth = dateOfBirth,
            DateArrived = clock.GetCurrentInstant().InZone(Const.TimeZone).Date,
            Image = imageData
        };
        uow.PuppyRepository.Add(puppy);
        await uow.SaveChangesAsync();
        logger.LogInformation("Successfully admitted puppy {PuppyName} to shelter {ShelterName}", puppy.Name,
                              shelter.Name);

        return new Success<Puppy>(puppy);
    }

    public async ValueTask<OneOf<Puppy, NotFound>> GetPuppyByIdAsync(int puppyId)
    {
        var puppy = await uow.PuppyRepository.GetByIdAsync(puppyId);
        if (puppy is null)
        {
            logger.LogWarning("Puppy with Id {Id} doesnt exist", puppyId);

            return new NotFound();
        }

        logger.LogTrace("Puppy with Id {Id} found", puppyId);

        return puppy;
    }

    public async ValueTask<OneOf<Success, NotFound>> DeletePuppyAsync(int puppyId)
    {
        var puppy = await uow.PuppyRepository.GetByIdAsync(puppyId);
        if (puppy is null)
        {
            logger.LogWarning("Puppy with Id {Id} doesnt exist", puppyId);

            return new NotFound();
        }

        uow.PuppyRepository.Remove(puppy);
        await uow.SaveChangesAsync();
        logger.LogInformation("Successfully deleted puppy with Id {Id}", puppyId);

        return new Success();
    }

    public async ValueTask<OneOf<Success<LocalDate>, NotFound, IPuppyService.AlreadyAdopted>>
        AdoptPuppyAsync(int puppyId, string adopterName)
    {
        var puppy = await uow.PuppyRepository.GetByIdAsync(puppyId);
        if (puppy is null)
        {
            logger.LogWarning("Puppy with Id {Id} doesnt exist", puppyId);

            return new NotFound();
        }

        if (puppy.DateAdopted != null)
        {
            logger.LogDebug("Puppy with Id {Id} was already adopted", puppyId);

            return new IPuppyService.AlreadyAdopted();
        }

        var today = clock.GetCurrentInstant().InZone(Const.TimeZone).Date;

        puppy.AdoptedBy = adopterName;
        puppy.DateAdopted = today;
        await uow.SaveChangesAsync();
        logger.LogInformation("Successfully adopted Puppy {PuppyName} by adopter {AdopterName}", puppy.Name,
                              adopterName);

        return new Success<LocalDate>(today);
    }
}