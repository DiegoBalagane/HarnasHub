using ErrorOr;
using HarnasHub.Application.Features.Veto.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Veto.GetVetoSuggestion;

/// <summary>Suggests picks and bans against an opponent from the map pool, the team's record and the opponent's recorded vetoes.</summary>
public record GetVetoSuggestionQuery(string OpponentName) : IRequest<ErrorOr<VetoSuggestionDto>>;
