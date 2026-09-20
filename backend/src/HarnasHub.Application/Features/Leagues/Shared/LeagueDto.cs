using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Leagues.Shared;

/// <summary>A named league season/division that groups several match results together.</summary>
public record LeagueDto(Guid Id, string Name, string Season, LeagueType Type);
