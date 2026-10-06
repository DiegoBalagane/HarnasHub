using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Tests.Common;

/// <summary>In-memory <see cref="IApplicationDbContext"/> used to exercise handlers without a real database.</summary>
public class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
	: DbContext(options), IApplicationDbContext
{
	#region Public Properties

	public DbSet<User> Users => Set<User>();
	public DbSet<Event> Events => Set<Event>();
	public DbSet<Core.Entities.Availability> Availabilities => Set<Core.Entities.Availability>();
	public DbSet<TaskItem> Tasks => Set<TaskItem>();
	public DbSet<MatchResult> MatchResults => Set<MatchResult>();
	public DbSet<MatchDemoAnalysis> MatchDemoAnalyses => Set<MatchDemoAnalysis>();
	public DbSet<NadeEntry> NadeEntries => Set<NadeEntry>();
	public DbSet<TrainingMaterial> TrainingMaterials => Set<TrainingMaterial>();
	public DbSet<PlayerMatchStat> PlayerMatchStats => Set<PlayerMatchStat>();
	public DbSet<OpponentNote> OpponentNotes => Set<OpponentNote>();
	public DbSet<PlayerAvailabilityDay> PlayerAvailabilityDays => Set<PlayerAvailabilityDay>();
	public DbSet<Vacation> Vacations => Set<Vacation>();
	public DbSet<MapPositionAssignment> MapPositionAssignments => Set<MapPositionAssignment>();
	public DbSet<MapTextAnnotation> MapTextAnnotations => Set<MapTextAnnotation>();
	public DbSet<UserSecondaryTeamRole> UserSecondaryTeamRoles => Set<UserSecondaryTeamRole>();
	public DbSet<Tactic> Tactics => Set<Tactic>();
	public DbSet<TacticPoint> TacticPoints => Set<TacticPoint>();
	public DbSet<Tournament> Tournaments => Set<Tournament>();
	public DbSet<League> Leagues => Set<League>();
	public DbSet<AnalysisBoard> AnalysisBoards => Set<AnalysisBoard>();
	public DbSet<AttendanceIncident> AttendanceIncidents => Set<AttendanceIncident>();
	public DbSet<MapPoolEntry> MapPoolEntries => Set<MapPoolEntry>();
	public DbSet<EventVetoStep> EventVetoSteps => Set<EventVetoStep>();
	public DbSet<EventGamePlan> EventGamePlans => Set<EventGamePlan>();
	public DbSet<EventGamePlanItem> EventGamePlanItems => Set<EventGamePlanItem>();
	public DbSet<OpponentFaceitLink> OpponentFaceitLinks => Set<OpponentFaceitLink>();
	public DbSet<FaceitPlayer> FaceitPlayers => Set<FaceitPlayer>();
	public DbSet<FaceitMatch> FaceitMatches => Set<FaceitMatch>();
	public DbSet<FaceitMatchPlayerStat> FaceitMatchPlayerStats => Set<FaceitMatchPlayerStat>();
	public DbSet<OpponentReportSnapshot> OpponentReportSnapshots => Set<OpponentReportSnapshot>();
	public DbSet<OpponentDemoAnalysis> OpponentDemoAnalyses => Set<OpponentDemoAnalysis>();
	public DbSet<BackgroundJob> BackgroundJobs => Set<BackgroundJob>();

	#endregion

	#region Public Methods

	/// <summary>Creates a context backed by a fresh, isolated in-memory database.</summary>
	public static TestApplicationDbContext Create()
	{
		var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
			.UseInMemoryDatabase($"HarnasHubTests-{Guid.NewGuid()}")
			.Options;

		return new TestApplicationDbContext(options);
	}

	#endregion
}
