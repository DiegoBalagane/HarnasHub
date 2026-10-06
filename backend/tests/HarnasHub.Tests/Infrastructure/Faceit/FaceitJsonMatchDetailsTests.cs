#region Usings

using System.Text.Json;
using HarnasHub.Infrastructure.Faceit;
using Xunit;

#endregion

namespace HarnasHub.Tests.Infrastructure.Faceit;

public class FaceitJsonMatchDetailsTests
{
	#region Public Methods

	[Fact]
	public void Should_read_competition_times_and_picked_maps_of_a_match()
	{
		using var document = JsonDocument.Parse("""
			{ "match_id": "1-abc", "competition_type": "championship", "competition_name": "ESEA Open",
			  "started_at": 1759320000, "finished_at": 1759323600,
			  "voting": { "map": { "pick": ["de_mirage", "de_nuke"] } },
			  "teams": { "faction1": { "name": "A", "roster": [] }, "faction2": { "name": "B", "roster": [] } } }
			""");

		var match = FaceitJson.ReadMatch(document.RootElement);

		Assert.NotNull(match);
		Assert.Equal("championship", match.CompetitionType);
		Assert.Equal("ESEA Open", match.CompetitionName);
		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1759320000).UtcDateTime, match.StartedAtUtc);
		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1759323600).UtcDateTime, match.FinishedAtUtc);
		Assert.Equal(["de_mirage", "de_nuke"], match.PickedMaps);
	}

	[Fact]
	public void Should_leave_details_empty_when_missing()
	{
		using var document = JsonDocument.Parse("""{ "match_id": "1-abc", "teams": { "faction1": { "roster": [] } } }""");

		var match = FaceitJson.ReadMatch(document.RootElement);

		Assert.NotNull(match);
		Assert.Null(match.CompetitionType);
		Assert.Null(match.StartedAtUtc);
		Assert.Empty(match.PickedMaps);
	}

	#endregion
}
