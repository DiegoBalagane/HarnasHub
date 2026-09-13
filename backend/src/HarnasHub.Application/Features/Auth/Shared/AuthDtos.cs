namespace HarnasHub.Application.Features.Auth.Shared;

/// <summary>Access token and basic profile returned after a successful register/login.</summary>
public record AuthResultDto(string AccessToken, Guid UserId, string DisplayName, string Role);
