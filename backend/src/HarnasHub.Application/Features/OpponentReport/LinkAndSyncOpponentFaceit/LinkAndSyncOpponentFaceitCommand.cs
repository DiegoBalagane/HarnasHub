#region Usings

using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;

/// <summary>Links an opponent to its FACEIT roster (see <c>LinkOpponentFaceitCommand</c>) and right away pulls the FACEIT data
/// for the report — the background-job form of "Powiąż" (previously two requests from the browser). Coach/Manager only,
/// enforced at the endpoint.</summary>
public record LinkAndSyncOpponentFaceitCommand(string OpponentName, string Source) : IRequest<ErrorOr<OpponentFaceitLinkDto>>;
