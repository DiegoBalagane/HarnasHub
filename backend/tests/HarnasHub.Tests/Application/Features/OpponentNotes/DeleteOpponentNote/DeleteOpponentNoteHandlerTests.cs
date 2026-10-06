using HarnasHub.Application.Features.OpponentNotes.DeleteOpponentNote;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.DeleteOpponentNote;

public class DeleteOpponentNoteHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_note()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var noteId = Guid.NewGuid();
		dbContext.OpponentNotes.Add(new OpponentNote
		{
			Id = noteId,
			OpponentName = "Team X",
			Content = "Treść",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new DeleteOpponentNoteHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteOpponentNoteCommand(noteId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.OpponentNotes);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_note()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteOpponentNoteHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteOpponentNoteCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("OpponentNotes.NoteNotFound", result.FirstError.Code);
	}

	#endregion
}
