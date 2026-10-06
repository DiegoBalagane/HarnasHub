namespace HarnasHub.Application.Abstractions;

/// <summary>One player blinded by a flashbang in an official round. <paramref name="IsTeamFlash"/> is true when thrower
/// and victim stood on the same side (a self-flash has the same player as both); the participants carry no position.</summary>
public record DemoBlind(
	int RoundNumber,
	float SecondsIntoRound,
	DemoKillParticipant Attacker,
	DemoKillParticipant Victim,
	float DurationSeconds,
	bool IsTeamFlash);
