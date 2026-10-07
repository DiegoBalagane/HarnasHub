using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.MatchAnalysis.ExcludePlayer;

/// <summary>Hides a player from one match's demo analysis (display only; the stored timeline is untouched). Coach/Manager only — enforced at the endpoint.</summary>
public record ExcludePlayerCommand(Guid MatchResultId, long SteamId64) : IRequest<ErrorOr<Success>>;
