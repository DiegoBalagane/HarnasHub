using HarnasHub.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace HarnasHub.Api.Hubs;

/// <summary>Broadcasts live-update signals to every connected client over <see cref="TeamHub"/>.</summary>
public class SignalRRealtimeNotifier(IHubContext<TeamHub> hubContext) : IRealtimeNotifier
{
	#region Public Methods

	public Task NotifyAsync(string topic, CancellationToken cancellationToken = default) =>
		hubContext.Clients.All.SendAsync("update", topic, cancellationToken);

	#endregion
}
