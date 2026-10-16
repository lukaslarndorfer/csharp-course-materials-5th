using Microsoft.Extensions.Logging;
using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Core.Services;

/// <summary>
///     Service for managing puppies in the shelter
/// </summary>
public interface IPuppyService
{
    /// <summary>
    ///     Gets basic information about all puppies in the given shelter
    /// </summary>
    /// <param name="shelter">The shelter for which to retrieve puppy data</param>
    /// <returns>Overview information about all puppies in the shelter</returns>
    public ValueTask<IReadOnlyCollection<PuppyInformation>> GetAllPuppiesAsync(Shelter shelter);

    /// <summary>
    ///     Admits a new puppy to the shelter with the given details
    /// </summary>
    /// <param name="shelter">The shelter to which to admit the puppy to</param>
    /// <param name="name">Name of the puppy</param>
    /// <param name="breed">Breed of the puppy</param>
    /// <param name="dateOfBirth">Date of birth of the puppy</param>
    /// <param name="imageData">Optionally, bytes of an image of the puppy</param>
    /// <returns>The created puppy entity</returns>
    public ValueTask<Puppy> AdmitPuppyAsync(Shelter shelter, string name, string breed,
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

internal sealed class PuppyService(IUnitOfWork uow, IDateTimeProvider dateTimeProvider, ILogger<PuppyService> logger)
    : ServiceBase, IPuppyService
{
    public async ValueTask<IReadOnlyCollection<PuppyInformation>> GetAllPuppiesAsync(Shelter shelter)
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    public async ValueTask<Puppy> AdmitPuppyAsync(Shelter shelter, string name, string breed,
                                                  LocalDate dateOfBirth, byte[]? imageData)
    {
        // Due to different scopes and database contexts for each view model, even entities loaded tracking
        // will not be attached to the context of a different view model.
        // Therefore, we either have to load it again here to set it, or only set the foreign key.
        // Otherwise, we would get a duplicate primary key error for the shelter when saving the new puppy.
        
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    public async ValueTask<OneOf<Puppy, NotFound>> GetPuppyByIdAsync(int puppyId)
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    public async ValueTask<OneOf<Success, NotFound>> DeletePuppyAsync(int puppyId)
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    public async ValueTask<OneOf<Success<LocalDate>, NotFound, IPuppyService.AlreadyAdopted>>
        AdoptPuppyAsync(int puppyId, string adopterName)
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }
}
