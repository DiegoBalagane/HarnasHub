using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentReport;

/// <summary>Returns the "them vs us" report of one opponent — the stored snapshot when there is one, otherwise a report built
/// from whatever is cached (possibly nothing yet).</summary>
public record GetOpponentReportQuery(string Name) : IRequest<ErrorOr<OpponentReportDto>>;
