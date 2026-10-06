#region Usings

using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.DownloadOpponentDemos;

/// <summary>Downloads (FACEIT Downloads API) and analyses the demos of the opponent's last <paramref name="Count"/> team games
/// not analysed yet, optionally only on <paramref name="Maps"/>. Runs as a background job; the count stays small since each demo is ~100 MB.
/// Coach/Manager only, enforced at the endpoint.</summary>
public record DownloadOpponentDemosCommand(string OpponentName, List<MapName>? Maps, int Count) : IRequest<ErrorOr<OpponentDemoDownloadResultDto>>
{
	/// <summary>Most demos one run may download (each is ~100 MB and takes a while to parse).</summary>
	public const int MaxCount = 5;
}
