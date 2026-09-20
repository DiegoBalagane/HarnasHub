using ErrorOr;
using HarnasHub.Application.Features.Nades.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Nades.UpdateNadePosition;

/// <summary>Sets or clears a nade entry's landing spot on its map's radar; both coordinates null clears the pin.</summary>
public record UpdateNadePositionCommand(Guid NadeId, float? X, float? Y) : IRequest<ErrorOr<NadeEntryDto>>;
