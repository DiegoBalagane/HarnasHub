namespace HarnasHub.Application.Features.Roster.Shared;

/// <summary>A single team member as shown in the roster.</summary>
public record TeamMemberDto(Guid Id, string DisplayName, string Role, string? AvatarUrl);
