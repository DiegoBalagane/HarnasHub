using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Match format of a simulated veto.</summary>
public enum VetoFormat
{
	/// <summary>Alternating bans until one map is left.</summary>
	Bo1 = 0,
	/// <summary>Ban, ban, pick, pick, ban, ban, decider.</summary>
	Bo3 = 1
}

/// <summary>One map as the simulation sees it: <paramref name="OurScore"/> from <c>VetoScoring</c> (higher = better for us),
/// <paramref name="TheirPreference"/> from <see cref="OpponentVetoPredictor"/>, short Polish notes used in the reasons, and
/// <paramref name="OurTier"/> — our ban order (maps we don't play before familiar maps, whatever the score).</summary>
public record VetoCandidate(MapName Map, int OurScore, double TheirPreference, string OurNote, string TheirNote, VetoBanTier OurTier = VetoBanTier.Keep);

/// <summary>Plays a veto out step by step: we ban by our tier then score (maps we don't play first) / pick our best familiar map,
/// they ban what they never play / pick their favourite.</summary>
public static class VetoSimulator
{
	#region Private Fields

	private static readonly VetoAction[] Bo3Pattern = [VetoAction.Ban, VetoAction.Ban, VetoAction.Pick, VetoAction.Pick, VetoAction.Ban, VetoAction.Ban];

	#endregion

	#region Public Methods

	/// <summary>The full sequence for <paramref name="format"/>; actors alternate starting with us (or them when <paramref name="weStart"/>
	/// is false) and the last remaining map is the decider.</summary>
	public static List<VetoPlanStepDto> Simulate(IReadOnlyCollection<VetoCandidate> maps, VetoFormat format, bool weStart = true)
	{
		var remaining = maps.ToList();
		var steps = new List<VetoPlanStepDto>();
		var hadUnplayed = remaining.Any(m => m.OurTier == VetoBanTier.NotPlayed);

		for (var index = 0; remaining.Count > 1; index++)
		{
			var action = format == VetoFormat.Bo3 && index < Bo3Pattern.Length ? Bo3Pattern[index] : VetoAction.Ban;
			var actor = (index % 2 == 0) == weStart ? VetoActor.Us : VetoActor.Opponent;
			var (chosen, reason) = Choose(remaining, actor, action, hadUnplayed);

			remaining.Remove(chosen);
			steps.Add(new VetoPlanStepDto(steps.Count + 1, actor.ToString(), action.ToString(), chosen.Map.ToString(), reason));
		}

		if (remaining.Count == 1)
		{
			var decider = remaining[0];
			steps.Add(new VetoPlanStepDto(
				steps.Count + 1,
				VetoActor.Us.ToString(),
				VetoAction.Decider.ToString(),
				decider.Map.ToString(),
				$"Zostaje jako decydująca — {decider.OurNote}; {decider.TheirNote}."));
		}

		return steps;
	}

	#endregion

	#region Private Methods

	/// <summary>The map the actor would take for the action, with the sentence explaining it; <paramref name="hadUnplayed"/> says
	/// whether the pool had maps we don't play (so a later ban of a familiar map is explained as "those are gone").</summary>
	private static (VetoCandidate Map, string Reason) Choose(List<VetoCandidate> remaining, VetoActor actor, VetoAction action, bool hadUnplayed)
	{
		if (actor == VetoActor.Us)
		{
			if (action == VetoAction.Pick)
			{
				var pick = remaining
					.OrderByDescending(m => m.OurTier >= VetoBanTier.BanCandidate)
					.ThenByDescending(m => m.OurScore)
					.ThenBy(m => m.TheirPreference)
					.ThenBy(m => m.Map)
					.First();
				return (pick, $"Najlepsza dla nas z pozostałych map — {pick.OurNote}; {pick.TheirNote}.");
			}

			var ban = remaining.OrderBy(m => m.OurTier).ThenBy(m => m.OurScore).ThenByDescending(m => m.TheirPreference).ThenBy(m => m.Map).First();
			var why = ban.OurTier switch
			{
				VetoBanTier.NotPlayed => Capitalize(ban.OurNote),
				>= VetoBanTier.BanCandidate when hadUnplayed => $"Mapy, których nie gramy, już odpadły — najsłabsza z pozostałych: {ban.OurNote}",
				_ => $"Najsłabsza dla nas z pozostałych map — {ban.OurNote}"
			};
			return (ban, $"{why}; {ban.TheirNote}.");
		}

		if (action == VetoAction.Pick)
		{
			var pick = remaining.OrderByDescending(m => m.TheirPreference).ThenBy(m => m.Map).First();
			return (pick, $"Przewidywany pick rywala — ich ulubiona z pozostałych ({pick.TheirNote}).");
		}

		var theirBan = remaining.OrderBy(m => m.TheirPreference).ThenByDescending(m => m.OurScore).ThenBy(m => m.Map).First();
		return (theirBan, $"Przewidywany ban rywala — grają ją najrzadziej z pozostałych ({theirBan.TheirNote}).");
	}

	/// <summary>The note with its first letter upper-cased.</summary>
	private static string Capitalize(string note) =>
		note.Length == 0 ? "Mapa, której nie gramy" : $"{char.ToUpperInvariant(note[0])}{note[1..]}";

	#endregion
}
