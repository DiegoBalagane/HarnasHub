#region Usings

using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;

/// <summary>Analyses one demo of the opponent previously uploaded through the presigned demo upload (one call per file
/// when several are uploaded). <paramref name="FileName"/> (the original name, optional) lets a FACEIT demo name its match,
/// which confirms the opponent's side. Coach/Manager only, enforced at the endpoint.</summary>
public record AnalyzeOpponentDemoCommand(string OpponentName, string ObjectKey, string? FileName = null) : IRequest<ErrorOr<OpponentDemoDto>>;
