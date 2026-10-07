using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.MatchAnalysis.IncludePlayer;

/// <summary>Brings a previously excluded player back into one match's demo analysis. Coach/Manager only — enforced at the endpoint.</summary>
public record IncludePlayerCommand(Guid MatchResultId, long SteamId64) : IRequest<ErrorOr<Success>>;
