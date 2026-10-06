using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentReport.SyncOurFaceit;

/// <summary>Resolves our players on FACEIT by their SteamID64 and caches their recent match history.</summary>
public record SyncOurFaceitCommand : IRequest<ErrorOr<FaceitSyncResultDto>>;
