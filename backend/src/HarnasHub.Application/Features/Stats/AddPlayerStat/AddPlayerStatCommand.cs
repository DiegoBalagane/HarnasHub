using ErrorOr;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Stats.AddPlayerStat;

/// <summary>Records one player's stat line for a match. Coach/Manager only — enforced at the endpoint.</summary>
public record AddPlayerStatCommand(
    Guid MatchResultId,
    Guid UserId,
    int Kills,
    int Deaths,
    int Assists,
    double Adr,
    double HeadshotPercentage,
    double Rating) : IRequest<ErrorOr<PlayerMatchStatDto>>;
