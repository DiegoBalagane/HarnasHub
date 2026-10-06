using HarnasHub.Application.Common.Notifications;
using Xunit;

namespace HarnasHub.Tests.Application.Common.Notifications;

public class DiscordMessageTests
{
	#region Public Methods

	[Fact]
	public void Truncate_leaves_short_messages_untouched()
	{
		Assert.Equal("hej", DiscordMessage.Truncate("hej"));
	}

	[Fact]
	public void Truncate_cuts_at_a_line_boundary_within_the_limit()
	{
		var text = string.Join('\n', Enumerable.Repeat(new string('a', 99), 30));

		var result = DiscordMessage.Truncate(text);

		Assert.True(result.Length <= DiscordMessage.MaxLength);
		Assert.EndsWith("…", result);
		Assert.All(result.TrimEnd('…').Split('\n'), line => Assert.Equal(99, line.Length));
	}

	[Fact]
	public void Truncate_cuts_a_single_long_word_hard()
	{
		var result = DiscordMessage.Truncate(new string('x', 5000));

		Assert.Equal(DiscordMessage.MaxLength, result.Length);
	}

	[Fact]
	public void Truncate_does_not_split_a_surrogate_pair()
	{
		var result = DiscordMessage.Truncate(string.Concat(Enumerable.Repeat("😀", 1500)));

		Assert.True(result.Length <= DiscordMessage.MaxLength);
		Assert.False(char.IsHighSurrogate(result[^2]));
	}

	[Fact]
	public void WithFooter_keeps_the_footer_and_shortens_only_the_body()
	{
		var footer = "\n🔗 https://hub.test/x";

		var result = DiscordMessage.WithFooter(new string('b', 3000) + " koniec", footer);

		Assert.True(result.Length <= DiscordMessage.MaxLength);
		Assert.EndsWith(footer, result);
	}

	[Fact]
	public void WithFooter_accepts_a_null_footer()
	{
		Assert.Equal("tekst", DiscordMessage.WithFooter("tekst", null));
	}

	#endregion
}
