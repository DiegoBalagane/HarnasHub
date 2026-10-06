#region Usings

using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentDemos;

/// <summary>Lists the analysed demos of one opponent, newest first, with what the demo section may offer.</summary>
public record GetOpponentDemosQuery(string Name) : IRequest<ErrorOr<OpponentDemosDto>>;
