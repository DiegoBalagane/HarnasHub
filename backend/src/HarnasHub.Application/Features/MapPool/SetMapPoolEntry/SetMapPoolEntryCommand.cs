using ErrorOr;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.MapPool.SetMapPoolEntry;

/// <summary>Sets (or, with a null <paramref name="Status"/>, clears) a map's place in the pool. Coach/Manager only — enforced at the endpoint.</summary>
public record SetMapPoolEntryCommand(MapName MapName, MapPoolStatus? Status, string? Note) : IRequest<ErrorOr<Success>>;
