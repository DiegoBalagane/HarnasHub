using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Abstractions;

/// <summary>Persistence contract the Application layer depends on, implemented by Infrastructure.</summary>
public interface IApplicationDbContext
{
	DbSet<User> Users { get; }
	DbSet<Event> Events { get; }
	DbSet<Availability> Availabilities { get; }
	DbSet<TaskItem> Tasks { get; }
	DbSet<MatchResult> MatchResults { get; }
	DbSet<NadeEntry> NadeEntries { get; }
	DbSet<TrainingMaterial> TrainingMaterials { get; }
	DbSet<PlayerMatchStat> PlayerMatchStats { get; }
	DbSet<OpponentNote> OpponentNotes { get; }
	DbSet<PlayerAvailabilityDay> PlayerAvailabilityDays { get; }
	DbSet<Vacation> Vacations { get; }
	DbSet<MapPositionAssignment> MapPositionAssignments { get; }
	DbSet<UserSecondaryTeamRole> UserSecondaryTeamRoles { get; }
	DbSet<Tactic> Tactics { get; }
	DbSet<TacticPoint> TacticPoints { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
