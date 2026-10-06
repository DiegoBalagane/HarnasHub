#region Usings

using ErrorOr;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;

/// <summary>Parses a demo uploaded through the presigned demo upload and stores it as the timeline of an already logged
/// result — how older matches get a timeline. Coach/Manager only, enforced at the endpoint.</summary>
public record AttachDemoToResultCommand(Guid MatchResultId, string ObjectKey) : IRequest<ErrorOr<MatchDemoAnalysisDto>>;

/// <summary>Summary of the stored timeline: how many rounds it has and whether "our" team was recognised.</summary>
public record MatchDemoAnalysisDto(Guid MatchResultId, int RoundsCount, int ParserVersion, bool OurTeamResolved);
