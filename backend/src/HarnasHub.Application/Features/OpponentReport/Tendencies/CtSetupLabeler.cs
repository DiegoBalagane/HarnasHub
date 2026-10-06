namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure naming of a CT setup from where its players stood: "2A-1M-2B" counts players per area, and a stack is
/// three or more players on one bombsite.</summary>
public static class CtSetupLabeler
{
	#region Public Fields

	/// <summary>Fewest spots with a known area for a round's setup to be labelled at all (deaths before the setup second
	/// or players outside the map's zones leave too little to describe).</summary>
	public const int MinKnownSpots = 3;

	/// <summary>Players on one site that make a stack.</summary>
	public const int StackSize = 3;

	#endregion

	#region Public Methods

	/// <summary>"{A}A-{Mid}M-{B}B", or null when fewer than <see cref="MinKnownSpots"/> spots have an area.</summary>
	public static string? Label(IEnumerable<MapArea?> areas)
	{
		var known = areas.Where(a => a.HasValue).Select(a => a!.Value).ToList();
		if (known.Count < MinKnownSpots)
		{
			return null;
		}

		return $"{known.Count(a => a == MapArea.A)}A-{known.Count(a => a == MapArea.Mid)}M-{known.Count(a => a == MapArea.B)}B";
	}

	/// <summary>The stacked bombsite, or null when neither site has <see cref="StackSize"/> players.</summary>
	public static MapArea? Stack(IEnumerable<MapArea?> areas)
	{
		var list = areas.ToList();
		if (list.Count(a => a == MapArea.A) >= StackSize)
		{
			return MapArea.A;
		}

		return list.Count(a => a == MapArea.B) >= StackSize ? MapArea.B : null;
	}

	/// <summary>The bombsite with fewer defenders in a "{A}A-{M}M-{B}B" label, or null on a tie / unparsable label.</summary>
	public static MapArea? WeakerSite(string label)
	{
		var parts = label.Split('-');
		if (parts.Length != 3
			|| !int.TryParse(parts[0].TrimEnd('A'), out var a)
			|| !int.TryParse(parts[2].TrimEnd('B'), out var b)
			|| a == b)
		{
			return null;
		}

		return a < b ? MapArea.A : MapArea.B;
	}

	#endregion
}
