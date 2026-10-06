#region Usings

using ErrorOr;
using HarnasHub.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;
using HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using HarnasHub.Tests.Common;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;

public class LinkAndSyncOpponentFaceitHandlerTests
{
	#region Private Fields

	private static readonly OpponentFaceitLinkDto Linked = new("Rivals", null, [], new DateTime(2026, 10, 6, 10, 0, 0, DateTimeKind.Utc), null);

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_link_then_sync_without_the_manual_cooldown_and_return_the_synced_link()
	{
		var synced = Linked with { LastSyncedAtUtc = new DateTime(2026, 10, 6, 10, 1, 0, DateTimeKind.Utc) };
		var sender = new RoutingSender(Linked, ReportWith(synced));
		var progress = new TestJobProgress();

		var result = await new LinkAndSyncOpponentFaceitHandler(sender, progress, NullLogger<LinkAndSyncOpponentFaceitHandler>.Instance)
			.Handle(new LinkAndSyncOpponentFaceitCommand("Rivals", "a, b, c"), CancellationToken.None);

		Assert.Equal(synced, result.Value);
		var sync = Assert.IsType<SyncOpponentFaceitCommand>(sender.Requests[1]);
		Assert.False(sync.IsManual);
		Assert.NotEmpty(progress.Reports);
	}

	[Fact]
	public async Task Should_keep_the_link_when_the_sync_fails()
	{
		var sender = new RoutingSender(Linked, OpponentReportErrors.FaceitUnavailable);

		var result = await new LinkAndSyncOpponentFaceitHandler(sender, new TestJobProgress(), NullLogger<LinkAndSyncOpponentFaceitHandler>.Instance)
			.Handle(new LinkAndSyncOpponentFaceitCommand("Rivals", "a, b, c"), CancellationToken.None);

		Assert.Equal(Linked, result.Value);
	}

	[Fact]
	public async Task Should_fail_without_syncing_when_the_link_fails()
	{
		var sender = new RoutingSender(OpponentReportErrors.EmptyRoster, OpponentReportErrors.FaceitUnavailable);

		var result = await new LinkAndSyncOpponentFaceitHandler(sender, new TestJobProgress(), NullLogger<LinkAndSyncOpponentFaceitHandler>.Instance)
			.Handle(new LinkAndSyncOpponentFaceitCommand("Rivals", "a, b, c"), CancellationToken.None);

		Assert.Equal(OpponentReportErrors.EmptyRoster.Code, result.FirstError.Code);
		Assert.Single(sender.Requests);
	}

	[Theory]
	[InlineData("Rivals", "nick1, nick2", true)]
	[InlineData("", "nick1, nick2", false)]
	[InlineData("Rivals", "", false)]
	public void Validator_should_apply_the_link_rules(string name, string source, bool valid)
	{
		Assert.Equal(valid, new LinkAndSyncOpponentFaceitCommandValidator().Validate(new LinkAndSyncOpponentFaceitCommand(name, source)).IsValid);
	}

	#endregion

	#region Private Methods

	private static OpponentReportDto ReportWith(OpponentFaceitLinkDto link) =>
		new("Rivals", true, link, DateTime.UtcNow, null, 0, 0, 0, 0, 0, [], [], [], [], new OpponentFormDto([], null, []), null, null);

	#endregion

	#region Nested Types

	/// <summary>Answers the link request with the first response and the sync request with the second.</summary>
	private sealed class RoutingSender(ErrorOr<OpponentFaceitLinkDto> link, ErrorOr<OpponentReportDto> sync) : ISender
	{
		public List<object> Requests { get; } = [];

		public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
		{
			Requests.Add(request);
			object response = request is LinkOpponentFaceitCommand ? link : sync;
			return Task.FromResult((TResponse)response);
		}

		public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
			throw new NotImplementedException();

		public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

		public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
			throw new NotImplementedException();

		public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
			throw new NotImplementedException();
	}

	#endregion
}
