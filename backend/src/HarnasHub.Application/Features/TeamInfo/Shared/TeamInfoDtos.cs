using HarnasHub.Core.Entities;

namespace HarnasHub.Application.Features.TeamInfo.Shared;

/// <summary>One entry of the team Info page as sent to team members.</summary>
public record TeamInfoEntryDto(Guid Id, string Category, string Title, string Value, bool IsSecret, int SortOrder, DateTime UpdatedAtUtc)
{
	/// <summary>Maps the entity to its DTO.</summary>
	public static TeamInfoEntryDto From(TeamInfoEntry entry) =>
		new(entry.Id, entry.Category, entry.Title, entry.Value, entry.IsSecret, entry.SortOrder, entry.UpdatedAtUtc);
}
