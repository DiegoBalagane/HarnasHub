using ErrorOr;
using HarnasHub.Application.Features.Nades.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Nades.GetNades;

/// <summary>Returns nade entries, optionally filtered by map and/or grenade type.</summary>
public record GetNadesQuery(string? MapName, GrenadeType? Type) : IRequest<ErrorOr<List<NadeEntryDto>>>;
