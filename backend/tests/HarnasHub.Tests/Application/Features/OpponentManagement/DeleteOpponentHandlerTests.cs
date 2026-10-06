using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.DeleteEvent;
using HarnasHub.Application.Features.OpponentManagement.DeleteOpponent;
using HarnasHub.Application.Features.Results.DeleteResult;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentNotes.OpponentTestData;

namespace HarnasHub.Tests.Application.Features.OpponentManagement;

public class DeleteOpponentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_scouting_data_and_timelines_and_leave_no_trace_when_there_is_no_history()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		storage.Objects["opponents/team-x/a.json.gz"] = [1];
		dbContext.OpponentNotes.Add(Note("TEAM X", DateTime.UtcNow));
		dbContext.OpponentNotes.Add(Note("Other", DateTime.UtcNow));
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink { Id = Guid.NewGuid(), OpponentKey = "team x", DisplayName = "Team X" });
		dbContext.OpponentReportSnapshots.Add(new OpponentReportSnapshot { Id = Guid.NewGuid(), OpponentKey = "team x" });
		dbContext.OpponentDemoAnalyses.Add(new OpponentDemoAnalysis { Id = Guid.NewGuid(), OpponentKey = "team x", TimelineObjectKey = "opponents/team-x/a.json.gz" });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var sender = new RecordingSender();

		var result = await CreateHandler(dbContext, sender, storage).Handle(new DeleteOpponentCommand("Team X", false), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.False(result.Value.Hidden);
		Assert.Equal(1, result.Value.DeletedNotes);
		Assert.Equal(1, result.Value.DeletedDemoAnalyses);
		Assert.Equal("Other", Assert.Single(dbContext.OpponentNotes).OpponentName);
		Assert.Empty(dbContext.OpponentFaceitLinks);
		Assert.Empty(dbContext.OpponentReportSnapshots);
		Assert.Empty(dbContext.OpponentDemoAnalyses);
		Assert.Empty(dbContext.HiddenOpponents);
		Assert.Equal(["opponents/team-x/a.json.gz"], storage.DeletedKeys);
		Assert.Empty(sender.Requests);
	}

	[Fact]
	public async Task Should_hide_instead_of_deleting_results_and_events_when_history_is_kept()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("Team X", DateTime.UtcNow));
		dbContext.MatchResults.Add(Match("Team X", 13, 5, DateTime.UtcNow));
		dbContext.Events.Add(Game("team x", DateTime.UtcNow.AddDays(1)));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var sender = new RecordingSender();

		var result = await CreateHandler(dbContext, sender).Handle(new DeleteOpponentCommand("Team X", false), CancellationToken.None);

		Assert.True(result.Value.Hidden);
		Assert.Equal(1, result.Value.DeletedNotes);
		Assert.Empty(dbContext.OpponentNotes);
		Assert.Equal(1, await dbContext.MatchResults.CountAsync());
		Assert.Equal(1, await dbContext.Events.CountAsync());
		Assert.Equal("team x", Assert.Single(dbContext.HiddenOpponents).OpponentKey);
		Assert.Empty(sender.Requests);
	}

	[Fact]
	public async Task Should_delete_results_and_events_through_their_own_commands_when_history_is_included()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = Match("Team X", 13, 5, DateTime.UtcNow);
		var game = Game("TEAM X", DateTime.UtcNow.AddDays(1));
		dbContext.MatchResults.AddRange(match, Match("Someone else", 1, 2, DateTime.UtcNow));
		dbContext.Events.Add(game);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var sender = new RecordingSender();

		var result = await CreateHandler(dbContext, sender).Handle(new DeleteOpponentCommand("Team X", true), CancellationToken.None);

		Assert.False(result.Value.Hidden);
		Assert.Equal((1, 1), (result.Value.DeletedMatchResults, result.Value.DeletedEvents));
		Assert.Contains(sender.Requests, r => r is DeleteResultCommand c && c.MatchResultId == match.Id);
		Assert.Contains(sender.Requests, r => r is DeleteEventCommand c && c.EventId == game.Id);
		Assert.Equal(2, sender.Requests.Count);
		Assert.Empty(dbContext.HiddenOpponents);
	}

	[Fact]
	public async Task Should_clear_a_previous_hidden_flag_when_nothing_is_kept()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentNotes.Add(Note("Team X", DateTime.UtcNow));
		dbContext.HiddenOpponents.Add(new HiddenOpponent { Id = Guid.NewGuid(), OpponentKey = "team x", DisplayName = "Team X" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await CreateHandler(dbContext, new RecordingSender()).Handle(new DeleteOpponentCommand("Team X", false), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.HiddenOpponents);
	}

	[Fact]
	public async Task Should_not_fail_when_deleting_a_timeline_throws()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentDemoAnalyses.Add(new OpponentDemoAnalysis { Id = Guid.NewGuid(), OpponentKey = "team x", TimelineObjectKey = "k" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await CreateHandler(dbContext, new RecordingSender(), new ThrowingStorage()).Handle(new DeleteOpponentCommand("team x", false), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.OpponentDemoAnalyses);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_opponent()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await CreateHandler(dbContext, new RecordingSender()).Handle(new DeleteOpponentCommand("Nobody", false), CancellationToken.None);

		Assert.Equal("OpponentManagement.NotFound", result.FirstError.Code);
	}

	[Fact]
	public void Validator_should_require_a_name_of_at_most_100_characters()
	{
		var validator = new DeleteOpponentCommandValidator();

		Assert.False(validator.Validate(new DeleteOpponentCommand("", false)).IsValid);
		Assert.False(validator.Validate(new DeleteOpponentCommand(new string('a', 101), false)).IsValid);
		Assert.True(validator.Validate(new DeleteOpponentCommand("Team X", true)).IsValid);
	}


	#endregion

	#region Private Methods

	private static DeleteOpponentHandler CreateHandler(TestApplicationDbContext dbContext, ISender sender, IFileStorage? storage = null) =>
		new(dbContext, sender, new TestCurrentUserService(Guid.NewGuid()), storage ?? new TestFileStorage(), new TestRealtimeNotifier(),
			NullLogger<DeleteOpponentHandler>.Instance);


	#endregion

	#region Nested Types

	/// <summary>Records every request and reports success, so the test can assert which delete commands were dispatched.</summary>
	private sealed class RecordingSender : ISender
	{
		public List<object> Requests { get; } = [];

		public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
		{
			Requests.Add(request);
			return Task.FromResult((TResponse)(object)(ErrorOr<Success>)Result.Success);
		}

		public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotImplementedException();

		public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

		public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
			throw new NotImplementedException();

		public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
	}

	/// <summary>Storage whose deletes always fail.</summary>
	private sealed class ThrowingStorage : IFileStorage
	{
		public bool IsConfigured => true;

		public Task<PresignedUpload> CreatePresignedUploadAsync(string keyPrefix, TimeSpan expiry, CancellationToken cancellationToken) => throw new NotImplementedException();

		public Task<string> CreatePresignedDownloadUrlAsync(string objectKey, TimeSpan expiry, CancellationToken cancellationToken) => throw new NotImplementedException();

		public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken) => throw new NotImplementedException();

		public Task DeleteAsync(string objectKey, CancellationToken cancellationToken) => throw new InvalidOperationException("boom");

		public Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken) => throw new NotImplementedException();

		public Task<int> DeleteOlderThanAsync(string keyPrefix, DateTime olderThanUtc, CancellationToken cancellationToken) => throw new NotImplementedException();
	}

	#endregion
}
