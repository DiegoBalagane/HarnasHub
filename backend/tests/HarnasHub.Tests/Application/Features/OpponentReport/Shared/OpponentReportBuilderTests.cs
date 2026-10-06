using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OpponentReportBuilderTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_build_an_empty_but_complete_report_without_any_data()
	{
		var report = OpponentReportBuilder.Build(Input([], [], []));

		Assert.Equal("Team X", report.OpponentName);
		Assert.Equal(0, report.TheirTeamGames);
		Assert.Equal(Enum.GetValues<MapName>().Length, report.Maps.Count);
		Assert.Equal(["Bo1", "Bo3"], report.VetoPlans.Select(p => p.Format));
		Assert.Contains(report.Insights, i => i.Kind == "LowSample");
		Assert.Empty(report.Form.LastGames);
	}

	[Fact]
	public void Should_compare_their_team_games_with_ours_including_internal_results()
	{
		var matches = new List<FaceitMatch>();
		// They: Mirage 6-0, Inferno 1-5. We (FACEIT): Inferno 3-0.
		matches.AddRange(Enumerable.Range(0, 6).Select(i => Match("de_mirage", Them, Strangers(), 13, 6, Now.AddDays(-i - 1))));
		matches.AddRange(Enumerable.Range(0, 6).Select(i => Match("de_inferno", Them, Strangers(), i == 0 ? 13 : 8, i == 0 ? 8 : 13, Now.AddDays(-i - 10))));
		matches.AddRange(Enumerable.Range(0, 3).Select(i => Match("de_inferno", Us, Strangers(), 13, 9, Now.AddDays(-i - 1))));
		// Internal: one Inferno win and one Mirage loss logged in HarnasHub.
		var veto = Enum.GetValues<MapName>()
			.Select(map => new MapVetoInput(map, null, map == MapName.Inferno ? 1 : 0, map == MapName.Mirage ? 1 : 0, 0, 0, 0, 0, 0, 0))
			.ToList();

		var report = OpponentReportBuilder.Build(Input(matches, [], veto));

		Assert.Equal(12, report.TheirTeamGames);
		Assert.Equal(3, report.OurTeamGames);
		Assert.Equal(2, report.OurInternalGames);
		var inferno = report.Maps.Single(m => m.MapName == "Inferno");
		Assert.Equal((6, 1, 4, 3, 1), (inferno.TheirGames, inferno.TheirWins, inferno.OurGames, inferno.OurFaceitGames, inferno.OurInternalGames));
		Assert.True(inferno.Advantage > 0);
		Assert.Equal("Medium", inferno.Confidence);
		Assert.Contains(inferno.VetoReasons, r => r.StartsWith("Słabość rywala: ich 17% wygranych przy 6 meczach"));
		Assert.Contains("Nasz bilans 4-0 — za mało danych (4 mecze), nie wpływa na rekomendację", inferno.VetoReasons);
		Assert.True(inferno.OurLowSample);
		Assert.Equal(3, inferno.OurFaceitWins);
		var mirage = report.Maps.Single(m => m.MapName == "Mirage");
		Assert.True(mirage.Advantage < 0);
		Assert.Equal("Pick", mirage.Prediction);
		// 12 team games are too few to call "they never play Nuke" a ban.
		var nuke = report.Maps.Single(m => m.MapName == "Nuke");
		Assert.Equal("Unknown", nuke.Prediction);
		Assert.Contains("za mało meczów", nuke.PredictionReason);
		// Most played by them first; ties broken by how much we played it.
		Assert.Equal(["Inferno", "Mirage"], report.Maps.Take(2).Select(m => m.MapName));
	}

	[Fact]
	public void Should_pick_players_to_watch_from_their_scoreboard_lines()
	{
		var games = Enumerable.Range(0, 3).Select(i => Match("de_mirage", Them, Strangers(), 13, 6, Now.AddDays(-i - 1))).ToList();
		var stats = games.SelectMany(g => new[] { Stat(g, "t1", 25, 10, 95), Stat(g, "t2", 12, 15, 60) }).ToList();

		var report = OpponentReportBuilder.Build(Input(games, stats, []));

		var mirage = Assert.Single(report.PlayersToWatch);
		Assert.Equal("t1", mirage.Players[0].PlayerId);
		Assert.Contains(report.Insights, i => i.Kind == "Player" && i.Text.Contains("nick-t1"));
	}

	[Fact]
	public void Should_add_individual_form_of_both_rosters_from_solo_and_team_lines()
	{
		var team = Enumerable.Range(0, 6).Select(i => Match("de_mirage", Them, Strangers(), 13, 6, Now.AddDays(-i - 1))).ToList();
		var solo = Them
			.SelectMany(id => Enumerable.Range(0, 10).Select(i => (Id: id, Game: Match("de_inferno", [id, .. Strangers()[..4]], Strangers(), 13, 10, Now.AddDays(-i - 10)))))
			.ToList();
		var ours = Enumerable.Range(0, 5).Select(i => Match("de_nuke", ["u1", .. Strangers()[..4]], Strangers(), 13, 3, Now.AddDays(-i - 1))).ToList();
		var theirStats = team.SelectMany(g => Them.Select(id => Stat(g, id, 20, 15, 80))).Concat(solo.Select(s => Stat(s.Game, s.Id, 25, 15, 90))).ToList();
		var input = Input([.. team, .. solo.Select(s => s.Game), .. ours], theirStats, []) with
		{
			OurStats = ours.Select(g => Stat(g, "u1", 30, 10, 110)).ToList(),
			OurPlayers = [new FaceitPlayerDto("u1", "Kacper", 2500, 10)]
		};

		var report = OpponentReportBuilder.Build(input);

		var individual = report.IndividualForm!;
		Assert.Equal(5, individual.Theirs.Players.Count);
		Assert.All(individual.Theirs.Players, p => Assert.Equal((16, 6, 10), (p.Games, p.TeamGames, p.SoloGames)));
		var inferno = individual.Theirs.MapComfort.Single(c => c.MapName == "Inferno");
		Assert.Equal((5, 5, 0), (inferno.RatedPlayers, inferno.RegularPlayers, inferno.AvoidingPlayers));
		var kacper = Assert.Single(individual.Ours.Players, p => p.Games > 0);
		Assert.Equal(("nick-u1", 2500, 5), (kacper.Nickname, kacper.Elo ?? 0, kacper.Maps.Single().SoloGames));
		// Barely played as a team, but all five play it regularly solo — no ban claim, just "niepewne".
		var row = report.Maps.Single(m => m.MapName == "Inferno");
		Assert.Equal("Unknown", row.Prediction);
		Assert.Contains("ale indywidualnie dużo (5 z 5 graczy gra ją regularnie solo)", row.PredictionReason);
	}

	[Fact]
	public void Should_limit_player_facing_numbers_to_the_active_lineup_of_a_team_page()
	{
		// A team page with 16 members: 6 play the recent team games (t1–t5 every game, sub "s1" in 3 of 10), "old1"/"old2" only
		// in games older than 60 days and outside the latest 10, the other 8 never.
		var actives = new[] { "t1", "t2", "t3", "t4", "s1" };
		var recent = Enumerable.Range(0, 10)
			.Select(i => Match("de_mirage", i < 3 ? actives : Them, Strangers(), 13, 6, Now.AddDays(-i - 1)))
			.ToList();
		var old = Enumerable.Range(0, 4)
			.Select(i => Match("de_ancient", ["old1", "old2", "t1", "t2", "t3"], Strangers(), 13, 9, Now.AddDays(-80 - i)))
			.ToList();
		var roster = Them.Concat(["s1", "old1", "old2"]).Concat(Enumerable.Range(0, 8).Select(i => $"x{i}")).ToHashSet();
		var oldSolo = Enumerable.Range(0, 3).Select(i => Match("de_nuke", ["old1", .. Strangers()[..4]], Strangers(), 13, 2, Now.AddDays(-5 - i))).ToList();
		var stats = recent.Concat(old).SelectMany(g => g.Team1PlayerIds.Where(roster.Contains).Select(id => Stat(g, id, id == "old1" ? 40 : 20, 15, id == "old1" ? 140 : 80)))
			.Concat(oldSolo.Select(g => Stat(g, "old1", 40, 5, 150)))
			.ToList();
		var input = new OpponentReportInput("Team X", null, Now, [.. recent, .. old, .. oldSolo], stats, roster, Us.ToHashSet(), []);

		var report = OpponentReportBuilder.Build(input);

		var lineup = report.ActiveLineup!;
		Assert.Equal(16, roster.Count);
		Assert.Equal(["s1", "t1", "t2", "t3", "t4", "t5"], lineup.Active.Select(p => p.PlayerId).Order());
		Assert.Contains("ostatnich 10 meczów drużynowych", lineup.Basis);
		Assert.Contains(lineup.Inactive, p => p.PlayerId == "old1" && p.TeamGames == 4 && p.RecentTeamGames == 0);
		Assert.Equal(14, report.TheirTeamGames);
		Assert.DoesNotContain(report.PlayersToWatch.SelectMany(m => m.Players), p => p.PlayerId.StartsWith("old"));
		Assert.DoesNotContain(report.IndividualForm!.Theirs.Players, p => p.PlayerId.StartsWith("old") || p.PlayerId.StartsWith("x"));
		Assert.DoesNotContain(report.Insights, i => i.Text.Contains("nick-old1"));
		// The sub who joined in the latest games is a real lineup change; ex-members never are.
		Assert.Equal(["nick-s1"], report.Form.NewPlayers);
	}

	[Fact]
	public void Should_show_lifetime_experience_of_the_active_lineup_and_downgrade_a_contradicted_ban()
	{
		// 20 team games, none on Ancient — but the lineup has a lot of lifetime Ancient.
		var games = Enumerable.Range(0, 20).Select(i => Match(i % 2 == 0 ? "de_mirage" : "de_inferno", Them, Strangers(), 13, 8, Now.AddDays(-i - 1))).ToList();
		var lifetime = Them.ToDictionary(
			id => id,
			id => (IReadOnlyList<HarnasHub.Application.Abstractions.FaceitLifetimeMapStats>)
			[
				new("de_ancient", 25, 13, 1.12),
				new("de_mirage", 28, 14, 1.01),
				new("de_nuke", 2, 1, 0.9)
			]);
		var input = Input(games, [], []) with { TheirLifetime = lifetime };

		var report = OpponentReportBuilder.Build(input);

		var ancient = report.Maps.Single(m => m.MapName == "Ancient");
		Assert.Equal((5, 125, true), (ancient.TheirLifetime!.Players, ancient.TheirLifetime.Matches, ancient.TheirLifetime.Experienced));
		Assert.Equal("Unknown", ancient.Prediction);
		Assert.Contains("125 meczów lifetime składu", ancient.PredictionReason);
		Assert.Equal("Ban", report.Maps.Single(m => m.MapName == "Nuke").Prediction);
		Assert.DoesNotContain(report.Insights, i => i.Kind == "LikelyBan" && i.Text.Contains("Ancient"));
	}

	#endregion

	#region Private Methods

	private static OpponentReportInput Input(List<FaceitMatch> matches, List<FaceitMatchPlayerStat> stats, List<MapVetoInput> veto) =>
		new("Team X", null, Now, matches, stats, Them.ToHashSet(), Us.ToHashSet(), veto);

	#endregion
}
