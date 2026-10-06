#region Usings

using System.Text.Json;
using System.Text.Json.Serialization;

#endregion

namespace HarnasHub.Application.Common.Jobs;

/// <summary>JSON settings for job payloads and results — the same web defaults (camelCase) and string enums the API uses,
/// so a stored result has exactly the shape the synchronous endpoint used to return.</summary>
public static class JobJson
{
	#region Public Properties

	/// <summary>Shared serializer options.</summary>
	public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	#endregion
}
