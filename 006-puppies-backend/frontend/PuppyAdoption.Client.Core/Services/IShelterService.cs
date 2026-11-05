using OneOf;
using OneOf.Types;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Client.Core.Services;

public interface IShelterService
{
    ValueTask<IReadOnlyList<ShelterDto>> GetAllSheltersAsync();

    ValueTask<OneOf<Success<ShelterDto>, NameAlreadyExists>> CreateNewShelterAsync(string name);

    readonly record struct NameAlreadyExists;
}
