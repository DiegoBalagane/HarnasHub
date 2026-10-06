using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;

/// <summary>Pulls fresh FACEIT data for a linked opponent and our team, then regenerates the report snapshot.
/// <paramref name="IsManual"/> applies the refresh cooldown (the background sync skips it).</summary>
public record SyncOpponentFaceitCommand(string OpponentName, bool IsManual) : IRequest<ErrorOr<OpponentReportDto>>;
