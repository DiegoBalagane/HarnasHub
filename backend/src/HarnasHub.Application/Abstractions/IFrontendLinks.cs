namespace HarnasHub.Application.Abstractions;

/// <summary>Builds absolute links to frontend pages for Discord messages; every method returns null when <c>Frontend:BaseUrl</c> is not configured.</summary>
public interface IFrontendLinks
{
	/// <summary>Link to the match page (<c>/results/{id}</c>).</summary>
	string? MatchResult(Guid matchResultId);

	/// <summary>Link to an opponent's report (<c>/opponents/report?name=…</c>).</summary>
	string? OpponentReport(string opponentName);
}
