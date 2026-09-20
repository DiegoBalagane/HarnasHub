using ErrorOr;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Stats.ImportStatsFromDemo;

/// <summary>Parses a CS2 demo and computes per-player stats; nothing is persisted here — Coach/Manager reviews the
/// result and saves each row individually via the existing <c>AddPlayerStat</c> slice.</summary>
public record ImportStatsFromDemoCommand(Stream DemoStream) : IRequest<ErrorOr<ImportStatsFromDemoResultDto>>;
