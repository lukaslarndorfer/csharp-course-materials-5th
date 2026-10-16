using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PuppyAdoption.Persistence.Model;

namespace PuppyAdoption.Persistence;

/// <summary>
///     Database context for the puppy adoption application
/// </summary>
/// <param name="options">Context configuration options</param>
public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public const string SchemaName = "puppy_adoption";

    public DbSet<Shelter> Shelters { get; set; }
    public DbSet<Puppy> Puppies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        EntityTypeBuilder<Shelter> shelter = modelBuilder.Entity<Shelter>();
        shelter.HasKey(s => s.Id);
        shelter.Property(s => s.Id).ValueGeneratedOnAdd();
        shelter.HasMany(s => s.Puppies)
               .WithOne(p => p.Shelter)
               .HasForeignKey(s => s.ShelterId)
               .OnDelete(DeleteBehavior.Cascade);
        shelter.HasIndex(s => s.Name).IsUnique();

        EntityTypeBuilder<Puppy> puppy = modelBuilder.Entity<Puppy>();
        puppy.HasKey(p => p.Id);
        puppy.Property(p => p.Id).ValueGeneratedOnAdd();
        puppy.HasIndex(p => p.DateAdopted);
    }
}
