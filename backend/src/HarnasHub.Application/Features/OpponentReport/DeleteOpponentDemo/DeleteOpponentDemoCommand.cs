#region Usings

using ErrorOr;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.DeleteOpponentDemo;

/// <summary>Deletes an analysed opponent demo together with its stored timeline. Coach/Manager only, enforced at the endpoint.</summary>
public record DeleteOpponentDemoCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
