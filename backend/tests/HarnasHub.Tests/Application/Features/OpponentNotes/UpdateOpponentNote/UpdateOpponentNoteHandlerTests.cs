using HarnasHub.Application.Features.OpponentNotes.UpdateOpponentNote;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentNotes.UpdateOpponentNote;

public class UpdateOpponentNoteHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_note_in_place()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var note = new OpponentNote
		{
			Id = Guid.NewGuid(),
			OpponentName = "Team X",
			Content = "Stara notatka",
			MaterialUrl = "https://example.com/old",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.OpponentNotes.Add(note);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new UpdateOpponentNoteHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(
			new UpdateOpponentNoteCommand(note.Id, " Team Y ", "Grają szybkie B na Mirage", ""),
			CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.OpponentNotes.SingleAsync();
		Assert.Equal("Team Y", stored.OpponentName);
		Assert.Equal("Grają szybkie B na Mirage", stored.Content);
		Assert.Null(stored.MaterialUrl);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_note()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateOpponentNoteHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(
			new UpdateOpponentNoteCommand(Guid.NewGuid(), "Team X", "Treść", null),
			CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("OpponentNotes.NoteNotFound", result.FirstError.Code);
	}

	#endregion
}
