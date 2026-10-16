using Microsoft.EntityFrameworkCore;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Persistence.Repositories;

/// <summary>
///     A repository for managing puppy entities
/// </summary>
public interface IPuppyRepository
{
    /// <summary>
    ///     Gets information about puppies which are (were) located in the given shelter.
    ///     Collection is sorted by adoption status (adopted first) and then by name.
    /// </summary>
    /// <param name="forShelter">The shelter for which to retrieve the puppies</param>
    /// <returns>A collection of basic information of all puppies</returns>
    public ValueTask<IReadOnlyCollection<PuppyInformation>> GetAllAsync(Shelter forShelter);

    /// <summary>
    ///     Attempts to retrieve a puppy by its unique ID.
    ///     Puppy IDs are unique across all shelters.
    /// </summary>
    /// <param name="id">The ID of the puppy</param>
    /// <returns>The puppy if found; null otherwise</returns>
    public ValueTask<Puppy?> GetByIdAsync(int id);

    /// <summary>
    ///     Adds a new puppy to the repository
    /// </summary>
    /// <param name="puppy">The puppy to add</param>
    public void Add(Puppy puppy);

    /// <summary>
    ///     Removes a puppy from the repository
    /// </summary>
    /// <param name="puppy">The puppy to remove</param>
    public void Remove(Puppy puppy);
}

internal sealed class PuppyRepository(DbSet<Puppy> puppies) : IPuppyRepository
{
    public async ValueTask<IReadOnlyCollection<PuppyInformation>> GetAllAsync(Shelter forShelter)
    {
        IQueryable<PuppyInformation> query = puppies.AsNoTracking()
                                                    .Where(p => p.ShelterId == forShelter.Id)
                                                    .Select(p => new { p.Id, p.Name, Adopted = p.DateAdopted != null })
                                                    .OrderByDescending(pi => pi.Adopted)
                                                    .ThenBy(pi => pi.Name)
                                                    .Select(a => new PuppyInformation(a.Id, a.Name, a.Adopted));

        return await query.ToListAsync();
    }

    public async ValueTask<Puppy?> GetByIdAsync(int id)
    {
        var puppy = await puppies.FindAsync(id);
        if (puppy is null)
        {
            return null;
        }

        // extra step, to ensure that the caches of different context instances of different view models are in sync
        await puppies.Entry(puppy).ReloadAsync();

        return puppy;
    }

    public void Add(Puppy puppy)
    {
        puppies.Add(puppy);
    }

    public void Remove(Puppy puppy)
    {
        puppies.Remove(puppy);
    }
}
