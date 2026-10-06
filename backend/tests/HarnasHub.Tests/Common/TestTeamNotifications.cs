using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Notifications;
using Microsoft.Extensions.Logging.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Builds a real <see cref="TeamNotifications"/> wired to test doubles so handler tests can assert what would be posted.</summary>
public static class TestTeamNotifications
{
	/// <summary>Creates the service; pass <paramref name="discord"/> to inspect the sent messages.</summary>
	public static TeamNotifications Create(
		IApplicationDbContext dbContext, IFileStorage? fileStorage = null, TestDiscordNotifier? discord = null, string? baseUrl = "https://hub.test") =>
		new(discord ?? new TestDiscordNotifier(), new TestFrontendLinks(baseUrl), dbContext, fileStorage ?? new TestFileStorage(), NullLogger<TeamNotifications>.Instance);
}
