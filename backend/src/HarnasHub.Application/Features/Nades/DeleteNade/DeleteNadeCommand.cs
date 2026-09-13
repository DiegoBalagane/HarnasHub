using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Nades.DeleteNade;

/// <summary>Deletes a nade entry. Allowed for its creator, or any Coach/Manager.</summary>
public record DeleteNadeCommand(Guid NadeId) : IRequest<ErrorOr<Success>>;
