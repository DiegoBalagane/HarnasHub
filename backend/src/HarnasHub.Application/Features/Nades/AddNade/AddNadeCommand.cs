using ErrorOr;
using HarnasHub.Application.Features.Nades.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Nades.AddNade;

/// <summary>Adds a nade lineup entry to the team's library. Any authenticated player can contribute.</summary>
public record AddNadeCommand(
	MapName MapName,
	GrenadeType Type,
	string Title,
	string? Description,
	string? YoutubeUrl) : IRequest<ErrorOr<NadeEntryDto>>;
