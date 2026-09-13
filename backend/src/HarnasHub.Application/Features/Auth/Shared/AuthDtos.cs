namespace HarnasHub.Application.Features.Auth.Shared;

/// <summary>Access token and basic profile returned after a successful Discord sign-in.</summary>
public record AuthResultDto(string AccessToken, Guid UserId, string DisplayName, string Role, string? AvatarUrl);
