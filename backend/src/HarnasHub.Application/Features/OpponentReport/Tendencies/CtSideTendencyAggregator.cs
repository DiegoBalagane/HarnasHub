namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure aggregation of the opponent's CT rounds across demos of one map.</summary>
public static class CtSideTendencyAggregator
{
	#region Public Fields

	/// <summary>Most setups listed.</summary>
	public const int MaxSetups = 5;

	/// <summary>Most heatmap points returned (keeps the report small with many demos).</summary>
	public const int MaxPoints = 400;

	#endregion

	#region Public Methods

	/// <summary>Aggregates the CT side; <paramref name="rounds"/> holds every round of every demo (both sides).</summary>
	public static CtSideTendenciesDto Aggregate(IReadOnlyList<IndexedRound> rounds)
	{
		var ct = rounds.Where(r => r.Round.Ct is not null).Select(r => (Facts: r.Round.Ct!, r.Round.Won)).ToList();

		var labelled = ct
			.Select(r => (Label: CtSetupLabeler.Label(r.Facts.Setup.Select(s => s.Area)), Stack: CtSetupLabeler.Stack(r.Facts.Setup.Select(s => s.Area))))
			.Where(r => r.Label is not null)
			.ToList();
		var setups = TSideTendencyAggregator.Shares(labelled.Select(r => r.Label!).ToList()).Take(MaxSetups).ToList();
		var stacks = labelled
			.Where(r => r.Stack is not null)
			.GroupBy(r => r.Stack!.Value)
			.Select(g => new TendencyShareDto(g.Key.ToString(), g.Count(), TendencyConfidence.Percent(g.Count(), labelled.Count)))
			.OrderByDescending(s => s.Count)
			.ToList();

		var setupPoints = ct
			.SelectMany(r => r.Facts.Setup)
			.Take(MaxPoints)
			.Select(s => new TendencyPointDto(s.X, s.Y, s.Area?.ToString(), s.Awp))
			.ToList();

		var awpKills = ct.SelectMany(r => r.Facts.AwpKills).ToList();
		var awpAreas = TSideTendencyAggregator.Shares(awpKills.Where(k => k.Area is not null).Select(k => k.Area!.Value.ToString()).ToList());
		var awpPoints = awpKills.Take(MaxPoints).Select(k => new TendencyPointDto(k.X, k.Y, k.Area?.ToString(), true)).ToList();

		var earlyRounds = ct.Count(r => r.Facts.EarlyKills > 0);
		var postPlant = ct.Where(r => r.Facts.PostPlant is PostPlantBehaviour.Retake or PostPlantBehaviour.Save).ToList();

		return new CtSideTendenciesDto(
			ct.Count,
			TendencyConfidence.For(ct.Count),
			setups,
			stacks,
			setupPoints,
			awpAreas,
			awpPoints,
			earlyRounds,
			TendencyConfidence.Percent(earlyRounds, ct.Count),
			postPlant.Count,
			postPlant.Count(r => r.Facts.PostPlant == PostPlantBehaviour.Retake),
			postPlant.Count(r => r.Facts.PostPlant == PostPlantBehaviour.Save),
			postPlant.Count(r => r.Facts.PostPlant == PostPlantBehaviour.Retake && r.Won == true));
	}

	#endregion
}
