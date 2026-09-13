using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Infrastructure.Database;

/// <summary>EF Core implementation of <see cref="IApplicationDbContext"/> backed by PostgreSQL.</summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    #region Public Properties

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    #endregion

    #region Protected Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    #endregion
}
