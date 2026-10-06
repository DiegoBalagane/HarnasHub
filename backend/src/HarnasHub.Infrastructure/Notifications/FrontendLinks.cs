#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Options;

#endregion

namespace HarnasHub.Infrastructure.Notifications;

/// <summary>Builds absolute frontend links from <c>Frontend:BaseUrl</c>; null when it is not configured.</summary>
public class FrontendLinks(IOptions<FrontendSettings> settings) : IFrontendLinks
{
	#region Public Methods

	/// <inheritdoc />
	public string? MatchResult(Guid matchResultId) => Build(settings.Value.BaseUrl, $"/results/{matchResultId}");

	/// <inheritdoc />
	public string? OpponentReport(string opponentName) =>
		Build(settings.Value.BaseUrl, $"/opponents/report?name={Uri.EscapeDataString(opponentName)}");

	/// <summary>Joins a base URL and a path; null when the base is empty.</summary>
	public static string? Build(string? baseUrl, string path)
	{
		var trimmed = baseUrl?.Trim().TrimEnd('/') ?? string.Empty;
		return trimmed.Length == 0 ? null : trimmed + path;
	}

	#endregion
}
