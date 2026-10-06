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
	DbSet<MatchDemoAnalysis> MatchDemoAnalyses { get; }
	DbSet<NadeEntry> NadeEntries { get; }
	DbSet<TrainingMaterial> TrainingMaterials { get; }
	DbSet<PlayerMatchStat> PlayerMatchStats { get; }
	DbSet<OpponentNote> OpponentNotes { get; }
	DbSet<PlayerAvailabilityDay> PlayerAvailabilityDays { get; }
	DbSet<Vacation> Vacations { get; }
	DbSet<MapPositionAssignment> MapPositionAssignments { get; }
	DbSet<MapTextAnnotation> MapTextAnnotations { get; }
	DbSet<UserSecondaryTeamRole> UserSecondaryTeamRoles { get; }
	DbSet<Tactic> Tactics { get; }
	DbSet<TacticPoint> TacticPoints { get; }
	DbSet<Tournament> Tournaments { get; }
	DbSet<League> Leagues { get; }
	DbSet<AnalysisBoard> AnalysisBoards { get; }
	DbSet<AttendanceIncident> AttendanceIncidents { get; }
	DbSet<MapPoolEntry> MapPoolEntries { get; }
	DbSet<EventVetoStep> EventVetoSteps { get; }
	DbSet<EventGamePlan> EventGamePlans { get; }
	DbSet<EventGamePlanItem> EventGamePlanItems { get; }
	DbSet<OpponentFaceitLink> OpponentFaceitLinks { get; }
	DbSet<FaceitPlayer> FaceitPlayers { get; }
	DbSet<FaceitMatch> FaceitMatches { get; }
	DbSet<FaceitMatchPlayerStat> FaceitMatchPlayerStats { get; }
	DbSet<OpponentReportSnapshot> OpponentReportSnapshots { get; }
	DbSet<OpponentDemoAnalysis> OpponentDemoAnalyses { get; }
	DbSet<BackgroundJob> BackgroundJobs { get; }
	DbSet<HiddenOpponent> HiddenOpponents { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
