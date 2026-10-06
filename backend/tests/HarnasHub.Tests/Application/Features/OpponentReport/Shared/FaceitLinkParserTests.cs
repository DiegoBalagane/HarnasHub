using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class FaceitLinkParserTests
{
	#region Public Methods

	[Fact]
	public void Should_recognise_a_team_url()
	{
		var source = FaceitLinkParser.Parse("https://www.faceit.com/en/teams/0B1C2D3E-1111-2222-3333-444455556666");

		Assert.NotNull(source);
		Assert.Equal(FaceitLinkKind.Team, source.Kind);
		Assert.Equal("0b1c2d3e-1111-2222-3333-444455556666", source.Id);
	}

	[Fact]
	public void Should_recognise_a_match_room_url_with_a_trailing_path()
	{
		var source = FaceitLinkParser.Parse(" https://www.faceit.com/pl/cs2/room/1-0b1c2d3e-1111-2222-3333-444455556666/scoreboard ");

		Assert.NotNull(source);
		Assert.Equal(FaceitLinkKind.Match, source.Kind);
		Assert.Equal("1-0b1c2d3e-1111-2222-3333-444455556666", source.Id);
	}

	[Fact]
	public void Should_split_nicknames_on_commas_spaces_and_newlines_without_duplicates()
	{
		var source = FaceitLinkParser.Parse("s1mple, ZywOo;\nNiKo  s1mple");

		Assert.NotNull(source);
		Assert.Equal(FaceitLinkKind.Nicknames, source.Kind);
		Assert.Equal(["s1mple", "ZywOo", "NiKo"], source.Nicknames);
	}

	[Theory]
	[InlineData("")]
	[InlineData("https://www.faceit.com/en/players/someone")]
	[InlineData("https://example.com/teams/abc")]
	[InlineData("nick with ünicode")]
	[InlineData("a b c d e f g h i j k")]
	public void Should_reject_anything_else(string text)
	{
		Assert.Null(FaceitLinkParser.Parse(text));
	}

	#endregion
}
