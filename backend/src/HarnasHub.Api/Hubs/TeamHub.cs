using HarnasHub.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HarnasHub.Api.Hubs;

/// <summary>
/// Live-update hub — the server pushes an "update" event with a topic string whenever team data changes,
/// and the frontend invalidates the matching cached query instead of polling.
/// </summary>
[Authorize(AuthorizationPolicies.TeamMember)]
public class TeamHub : Hub;
