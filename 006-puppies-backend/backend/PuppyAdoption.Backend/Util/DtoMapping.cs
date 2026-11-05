using PuppyAdoption.Backend.Persistence.Model;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Backend.Util;

/// <summary>
///     Maps persistence models to the DTOs of the API contract
/// </summary>
public static class DtoMapping
{
    extension(PuppyInformation puppy)
    {
        /// <summary>
        ///     Maps the puppy information to its DTO
        /// </summary>
        public PuppyInformationDto ToDto() => new(puppy.Id, puppy.Name, puppy.Adopted);
    }

    extension(Puppy puppy)
    {
        /// <summary>
        ///     Maps the puppy entity to its DTO
        /// </summary>
        public PuppyDto ToDto() => new(puppy.Id, puppy.Name, puppy.Breed, puppy.DateOfBirth, puppy.DateArrived,
                                       puppy.DateAdopted, puppy.AdoptedBy, puppy.Image);
    }

    extension(Shelter shelter)
    {
        /// <summary>
        ///     Maps the shelter entity to its DTO
        /// </summary>
        public ShelterDto ToDto() => new(shelter.Id, shelter.Name);
    }
}
