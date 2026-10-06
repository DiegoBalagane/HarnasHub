using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IFrontendLinks"/> with a fixed base URL (null = unconfigured, so no links).</summary>
public class TestFrontendLinks(string? baseUrl = "https://hub.test") : IFrontendLinks
{
	public string? MatchResult(Guid matchResultId) => baseUrl is null ? null : $"{baseUrl}/results/{matchResultId}";

	public string? OpponentReport(string opponentName) => baseUrl is null ? null : $"{baseUrl}/opponents/report?name={Uri.EscapeDataString(opponentName)}";
}
