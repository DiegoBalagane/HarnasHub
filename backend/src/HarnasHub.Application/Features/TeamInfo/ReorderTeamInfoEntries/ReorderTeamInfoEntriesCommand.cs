using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.TeamInfo.ReorderTeamInfoEntries;

/// <summary>Rewrites the sort order of the listed entries to match <paramref name="OrderedIds"/> (one category's new order). Coach/Manager only — enforced at the endpoint.</summary>
public record ReorderTeamInfoEntriesCommand(List<Guid> OrderedIds) : IRequest<ErrorOr<Success>>;
