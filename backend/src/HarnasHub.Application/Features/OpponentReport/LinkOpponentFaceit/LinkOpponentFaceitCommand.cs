using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;

/// <summary>Links an opponent to its FACEIT roster from a pasted team URL, match room URL or nickname list, replacing any
/// previous link. Coach/Manager only — enforced at the endpoint.</summary>
public record LinkOpponentFaceitCommand(string OpponentName, string Source) : IRequest<ErrorOr<OpponentFaceitLinkDto>>;
