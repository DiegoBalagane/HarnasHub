#region Usings

using HarnasHub.Infrastructure.Demos.Collectors;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos.Collectors;

public class DemoTeamsRosterTests
{
	#region Public Methods

	[Fact]
	public void Should_drop_roster_members_without_a_live_pawn_at_freeze_end()
	{
		var roster = DemoTeams.FilterPlaying([1, 2, 99], new HashSet<long> { 1, 2 });

		Assert.Equal([1L, 2L], roster);
	}

	[Fact]
	public void Should_keep_bots_and_leave_the_roster_untouched_when_freeze_end_was_not_observed()
	{
		Assert.Equal([1L, 99L], DemoTeams.FilterPlaying([1, 99], null));
		Assert.Equal([0L, 1L], DemoTeams.FilterPlaying([0, 1, 99], new HashSet<long> { 1 }));
	}

	#endregion
}
