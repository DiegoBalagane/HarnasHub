#region Usings

using System.Text.Json;
using System.Text.Json.Serialization;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>JSON (de)serialization of <see cref="OpponentDemoFacts"/> for the analysis row; enums as names so reordering
/// one never changes stored meaning.</summary>
public static class OpponentFactsSerializer
{
	#region Private Fields

	private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() },
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	#endregion

	#region Public Methods

	/// <summary>Facts as compact JSON.</summary>
	public static string Serialize(OpponentDemoFacts facts) => JsonSerializer.Serialize(facts, Options);

	/// <summary>Facts from JSON; null for empty/unreadable JSON or facts of a newer version than this code understands.</summary>
	public static OpponentDemoFacts? Deserialize(string? json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		try
		{
			var facts = JsonSerializer.Deserialize<OpponentDemoFacts>(json, Options);
			return facts is null || facts.Version > OpponentDemoFacts.CurrentVersion ? null : facts;
		}
		catch (JsonException)
		{
			return null;
		}
	}

	#endregion
}
