#region Usings

using HarnasHub.Application.Common.Jobs;
using HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;
using HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;
using HarnasHub.Application.Features.OpponentReport.DownloadOpponentDemos;
using HarnasHub.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;
using HarnasHub.Application.Features.Results.AnalyzeUploadedDemo;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Application.Features.Tactics.ExtractDemoNades;
using HarnasHub.Application.Features.Tactics.Shared;

#endregion

namespace HarnasHub.Application.Features.Jobs.Shared;

/// <summary>Every operation that runs as a background job. Adding one = a new entry here plus an endpoint that sends
/// <c>StartJobCommand</c> with the request; the request's own handler stays a plain MediatR handler.</summary>
public static class JobRegistry
{
	#region Private Fields

	private const string DownloadingDemo = "Pobieranie demki z magazynu";
	private static readonly TimeSpan DemoTimeout = TimeSpan.FromMinutes(15);

	private static readonly IJobDefinition[] Definitions =
	[
		new JobDefinition<AnalyzeUploadedDemoCommand, AnalyzeDemoResultDto>(
			JobKinds.AnalyzeUploadedDemo, JobLane.Demo, "Przygotowanie demki", DemoTimeout),
		new JobDefinition<AnalyzeDemoFromStorageCommand, AnalyzeDemoResultDto>(
			JobKinds.AnalyzeStoredDemo, JobLane.Demo, DownloadingDemo, DemoTimeout),
		new JobDefinition<AttachDemoToResultCommand, MatchDemoAnalysisDto>(
			JobKinds.AttachDemoToResult, JobLane.Demo, DownloadingDemo, DemoTimeout),
		new JobDefinition<ExtractDemoNadesCommand, DemoNadesDto>(
			JobKinds.ExtractDemoNades, JobLane.Demo, DownloadingDemo, DemoTimeout),
		new JobDefinition<AnalyzeOpponentDemoCommand, OpponentDemoDto>(
			JobKinds.AnalyzeOpponentDemo, JobLane.Demo, DownloadingDemo, DemoTimeout),
		new JobDefinition<DownloadOpponentDemosCommand, OpponentDemoDownloadResultDto>(
			JobKinds.DownloadOpponentDemos, JobLane.Demo, "Wyszukiwanie meczów do pobrania", TimeSpan.FromMinutes(60)),
		new JobDefinition<LinkAndSyncOpponentFaceitCommand, OpponentFaceitLinkDto>(
			JobKinds.LinkOpponentFaceit, JobLane.Faceit, "Wyszukiwanie graczy na FACEIT", TimeSpan.FromMinutes(10)),
		new JobDefinition<SyncOpponentFaceitCommand, OpponentReportDto>(
			JobKinds.RefreshOpponentFaceit, JobLane.Faceit, "Łączenie z FACEIT", TimeSpan.FromMinutes(10))
	];

	private static readonly Dictionary<string, IJobDefinition> ByKindMap = Definitions.ToDictionary(d => d.Kind);
	private static readonly Dictionary<Type, IJobDefinition> ByRequestTypeMap = Definitions.ToDictionary(d => d.RequestType);

	#endregion

	#region Public Properties

	/// <summary>All registered job kinds.</summary>
	public static IReadOnlyList<IJobDefinition> All => Definitions;

	#endregion

	#region Public Methods

	/// <summary>The definition of a stored kind; null for an unknown (e.g. removed) kind.</summary>
	public static IJobDefinition? ByKind(string kind) => ByKindMap.GetValueOrDefault(kind);

	/// <summary>The definition executing <paramref name="requestType"/>; null when that request isn't a job.</summary>
	public static IJobDefinition? ByRequestType(Type requestType) => ByRequestTypeMap.GetValueOrDefault(requestType);

	#endregion
}

/// <summary>Stable identifiers of job kinds, stored on <c>BackgroundJob.Kind</c> and sent to the frontend.</summary>
public static class JobKinds
{
	#region Public Fields

	/// <summary>Demo uploaded straight through the API, analysed for a new result.</summary>
	public const string AnalyzeUploadedDemo = "results.analyze-uploaded-demo";

	/// <summary>Demo uploaded to object storage, analysed for a new result.</summary>
	public const string AnalyzeStoredDemo = "results.analyze-stored-demo";

	/// <summary>Demo attached as the timeline of an existing result.</summary>
	public const string AttachDemoToResult = "match-analysis.attach-demo";

	/// <summary>Grenades extracted from a demo for the tactic import wizard.</summary>
	public const string ExtractDemoNades = "tactics.extract-demo-nades";

	/// <summary>One uploaded opponent demo analysed.</summary>
	public const string AnalyzeOpponentDemo = "opponents.analyze-demo";

	/// <summary>Opponent demos downloaded from FACEIT and analysed.</summary>
	public const string DownloadOpponentDemos = "opponents.download-demos";

	/// <summary>Opponent linked to a FACEIT roster, then synced.</summary>
	public const string LinkOpponentFaceit = "opponents.faceit-link";

	/// <summary>Manual refresh of an opponent's FACEIT data.</summary>
	public const string RefreshOpponentFaceit = "opponents.faceit-refresh";

	#endregion
}
