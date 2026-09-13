namespace HarnasHub.Application.Abstractions;

/// <summary>Identifies the user making the current request.</summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Role { get; }
}
