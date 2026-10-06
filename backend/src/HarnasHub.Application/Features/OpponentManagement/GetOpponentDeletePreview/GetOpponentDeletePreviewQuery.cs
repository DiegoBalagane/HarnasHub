using ErrorOr;
using HarnasHub.Application.Features.OpponentManagement.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentManagement.GetOpponentDeletePreview;

/// <summary>Counts what deleting the opponent <paramref name="Name"/> would remove, for the confirmation dialog.</summary>
public record GetOpponentDeletePreviewQuery(string Name) : IRequest<ErrorOr<OpponentDeletePreviewDto>>;
