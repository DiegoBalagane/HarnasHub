using ErrorOr;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentNotes.GetOpponentProfile;

/// <summary>Returns the full profile of one opponent, matched by name case- and whitespace-insensitively.</summary>
public record GetOpponentProfileQuery(string Name) : IRequest<ErrorOr<OpponentProfileDto>>;
