#region Usings

using HarnasHub.Application.Common.Faceit;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Faceit;

public class FaceitDemoFileNameTests
{
	#region Private Fields

	private const string MatchId = "1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e";

	#endregion

	#region Public Methods

	[Theory]
	[InlineData("1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-1-2.dem", 2)]
	[InlineData("1-8DA435DC-78FC-42F1-85CA-02F3E4D02A7E-1-1.dem", 1)]
	[InlineData("1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-3.dem", 3)]
	[InlineData("1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e.dem", null)]
	[InlineData("1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-1-2.dem.gz", 2)]
	[InlineData("1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-1-2 (1).dem", 2)]
	[InlineData("C:\\fakepath\\1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-1-2.dem", 2)]
	public void Should_recognise_faceit_demo_names(string fileName, int? mapNumber)
	{
		var reference = FaceitDemoFileName.Parse(fileName);

		Assert.NotNull(reference);
		Assert.Equal(MatchId, reference.MatchId);
		Assert.Equal(mapNumber, reference.MapNumber);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("mirage_scrim.dem")]
	[InlineData("8da435dc-78fc-42f1-85ca-02f3e4d02a7e.dem")]
	[InlineData("1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-final.dem")]
	[InlineData("x1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-1-2.dem")]
	public void Should_ignore_other_names(string? fileName)
	{
		Assert.Null(FaceitDemoFileName.Parse(fileName));
	}

	[Theory]
	[InlineData("matchmaking", "5v5 RANKED", MatchCategory.Scrimmage)]
	[InlineData("hub", "Polish Hub", MatchCategory.Scrimmage)]
	[InlineData(null, null, MatchCategory.Scrimmage)]
	[InlineData("championship", "ESEA Open S55", MatchCategory.League)]
	[InlineData("championship", "Polska Liga Esportowa", MatchCategory.League)]
	[InlineData("Championship", "Weekend Cup", MatchCategory.Tournament)]
	[InlineData("championship", "S59 EU Open10 D - Regular Season", MatchCategory.League)]
	[InlineData("championship", "Summer Cup S1 Finals", MatchCategory.Tournament)]
	public void Should_map_competitions_to_categories_conservatively(string? type, string? name, MatchCategory expected)
	{
		Assert.Equal(expected, FaceitCompetitionCategory.Map(type, name));
	}

	#endregion
}
