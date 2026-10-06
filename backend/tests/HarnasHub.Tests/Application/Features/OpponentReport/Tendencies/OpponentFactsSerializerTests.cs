#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class OpponentFactsSerializerTests
{
	#region Public Methods

	[Fact]
	public void Should_round_trip_facts_with_enums_as_names()
	{
		var facts = Facts(
			[TRound(2, MapArea.B, 70f, grenades: new GrenadeFact(DemoGrenadeType.Molotov, 0.1f, 0.2f, 30f)), CtRound(14, [MapArea.A, null], postPlant: PostPlantBehaviour.Retake)],
			[new OpponentPlayerFacts(11, "a", 2, 1, 1, 0, 0, 0, 0)]);

		var json = OpponentFactsSerializer.Serialize(facts);
		var back = OpponentFactsSerializer.Deserialize(json);

		Assert.Contains("\"Molotov\"", json);
		Assert.NotNull(back);
		Assert.Equal(MapArea.B, back.Rounds[0].T!.Target);
		Assert.Equal(PostPlantBehaviour.Retake, back.Rounds[1].Ct!.PostPlant);
		Assert.Null(back.Rounds[1].Ct!.Setup[1].Area);
		Assert.Equal("a", back.Players[0].Name);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("{not json")]
	[InlineData("{\"version\":999,\"rounds\":[],\"players\":[]}")]
	public void Should_return_null_for_unusable_json(string? json)
	{
		Assert.Null(OpponentFactsSerializer.Deserialize(json));
	}

	#endregion
}
