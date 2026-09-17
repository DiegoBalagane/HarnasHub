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
	public DbSet<MatchResult> MatchResults => Set<MatchResult>();
	public DbSet<NadeEntry> NadeEntries => Set<NadeEntry>();
	public DbSet<TrainingMaterial> TrainingMaterials => Set<TrainingMaterial>();
	public DbSet<PlayerMatchStat> PlayerMatchStats => Set<PlayerMatchStat>();
	public DbSet<OpponentNote> OpponentNotes => Set<OpponentNote>();
	public DbSet<PlayerAvailabilityDay> PlayerAvailabilityDays => Set<PlayerAvailabilityDay>();
	public DbSet<Vacation> Vacations => Set<Vacation>();
	public DbSet<MapPositionAssignment> MapPositionAssignments => Set<MapPositionAssignment>();
	public DbSet<UserSecondaryTeamRole> UserSecondaryTeamRoles => Set<UserSecondaryTeamRole>();
	public DbSet<Tactic> Tactics => Set<Tactic>();
	public DbSet<TacticPoint> TacticPoints => Set<TacticPoint>();

	#endregion

	#region Protected Methods

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}

	#endregion
}
