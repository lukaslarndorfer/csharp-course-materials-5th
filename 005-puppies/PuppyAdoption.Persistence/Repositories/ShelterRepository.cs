using Microsoft.EntityFrameworkCore;
using PuppyAdoption.Persistence.Model;

namespace PuppyAdoption.Persistence.Repositories;

/// <summary>
///     A repository for managing shelter entities
/// </summary>
public interface IShelterRepository
{
    /// <summary>
    ///     Adds a new shelter to the repository
    /// </summary>
    /// <param name="shelter">The shelter to add</param>
    public void Add(Shelter shelter);

    /// <summary>
    ///     Retrieves all shelters from the repository, ordered by name
    /// </summary>
    /// <returns>A collection of all shelters</returns>
    public ValueTask<IReadOnlyCollection<Shelter>> GetAllAsync();

    /// <summary>
    ///     Attempts to retrieve a shelter by its unique ID
    /// </summary>
    /// <param name="id">The ID of the shelter</param>
    /// <returns>The shelter if found; null otherwise</returns>
    public ValueTask<Shelter?> GetByIdAsync(int id);

    /// <summary>
    ///     Checks if a shelter with the given name already exists in the repository
    /// </summary>
    /// <param name="name">The name to test</param>
    /// <returns>True if the name is already in use; false otherwise</returns>
    public ValueTask<bool> CheckNameExistsAsync(string name);
}

internal sealed class ShelterRepository(DbSet<Shelter> shelters) : IShelterRepository
{
    public void Add(Shelter shelter)
    {
        shelters.Add(shelter);
    }

    public async ValueTask<IReadOnlyCollection<Shelter>> GetAllAsync()
    {
        IOrderedQueryable<Shelter> query = shelters
            .OrderBy(s => s.Name);

        return await query.ToListAsync();
    }

    public async ValueTask<Shelter?> GetByIdAsync(int id)
    {
        var shelter = await shelters.FindAsync(id);
        if (shelter is null)
        {
            return null;
        }

        // extra step, to ensure that the caches of different context instances of different view models are in sync
        await shelters.Entry(shelter).ReloadAsync();

        return shelter;
    }

    public async ValueTask<bool> CheckNameExistsAsync(string name)
    {
        bool exists = await shelters
            .AnyAsync(s => s.Name == name);

        return exists;
    }
}
