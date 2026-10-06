namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>One recorded veto step; <paramref name="Actor"/>, <paramref name="Action"/> and <paramref name="MapName"/> are enum names.</summary>
public record VetoStepDto(int Order, string Actor, string Action, string MapName);

/// <summary>A map's place in the suggested veto: <paramref name="Recommendation"/> is "Pick", "Ban" or "Neutral", and
/// <paramref name="Reasons"/> explain the <paramref name="Score"/> in plain Polish so the coach can overrule it knowingly.</summary>
public record MapVetoSuggestionDto(string MapName, int Score, string Recommendation, List<string> Reasons);

/// <summary>How often the opponent picked or banned a map in vetoes recorded against them.</summary>
public record OpponentMapTendencyDto(string MapName, int Picks, int Bans);

/// <summary>The veto suggestion against one opponent: every pool map, best pick first and first ban last, plus the
/// opponent's recorded tendencies and how many of their vetoes those are based on.</summary>
public record VetoSuggestionDto(
	string OpponentName,
	List<MapVetoSuggestionDto> Maps,
	List<OpponentMapTendencyDto> OpponentTendencies,
	int RecordedOpponentVetoes);
