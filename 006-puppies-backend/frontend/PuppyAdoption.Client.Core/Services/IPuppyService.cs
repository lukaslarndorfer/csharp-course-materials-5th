using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Client.Core.Services;

public interface IPuppyService
{
    ValueTask<OneOf<Success<PuppyInformationResponse>, NotFound>> GetAllPuppiesAsync(int shelterId);

    ValueTask<OneOf<PuppyDto, NotFound>> GetPuppyByIdAsync(int puppyId);

    ValueTask<OneOf<Success<PuppyDto>, NotFound>> AdmitPuppyAsync(int shelterId, string name, string breed,
                                                                  LocalDate dateOfBirth, byte[]? imageData);

    ValueTask<OneOf<Success<LocalDate>, NotFound, AlreadyAdopted>> AdoptPuppyAsync(int puppyId, string adopterName);

    ValueTask<OneOf<Success, NotFound>> DeletePuppyAsync(int puppyId);

    readonly record struct AlreadyAdopted;
}