using ErrorOr;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Stats.AddPlayerStat;

/// <summary>Records one player's stat line for a match. Coach/Manager only — enforced at the endpoint.
/// Everything from <paramref name="EntryKills"/> onward is optional — the manual entry form never sends it, only a demo import does.</summary>
public record AddPlayerStatCommand(
	Guid MatchResultId,
	Guid UserId,
	int Kills,
	int Deaths,
	int Assists,
	double Adr,
	double HeadshotPercentage,
	double Rating,
	int? EntryKills = null,
	int? EntryDeaths = null,
	double? KastPercentage = null,
	int? MultiKill2K = null,
	int? MultiKill3K = null,
	int? MultiKill4K = null,
	int? MultiKill5K = null,
	int? UtilityDamage = null,
	int? FlashAssists = null) : IRequest<ErrorOr<PlayerMatchStatDto>>;
