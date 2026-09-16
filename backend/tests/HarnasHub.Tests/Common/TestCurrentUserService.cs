using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="ICurrentUserService"/> returning a fixed identity for handler tests.</summary>
public class TestCurrentUserService(Guid userId, string role = "Player") : ICurrentUserService
{
	#region Public Properties

	public Guid UserId { get; } = userId;

	public string Role { get; } = role;

	#endregion
}
