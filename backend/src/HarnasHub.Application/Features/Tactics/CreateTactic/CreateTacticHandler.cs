using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.Tactics.CreateTactic;

/// <summary>Handles <see cref="CreateTacticCommand"/> by persisting a new, empty tactic.</summary>
public class CreateTacticHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<CreateTacticCommand, ErrorOr<TacticDetailDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TacticDetailDto>> Handle(CreateTacticCommand request, CancellationToken cancellationToken)
	{
		var tactic = new Tactic
		{
			Id = Guid.NewGuid(),
			MapName = request.MapName,
			Side = request.Side,
			Name = request.Name,
			Economy = request.Economy,
			Note = request.Note,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Tactics.Add(tactic);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tactics", cancellationToken);

		return new TacticDetailDto(
			tactic.Id, tactic.MapName, tactic.Side, tactic.Name, tactic.Economy.ToString(), tactic.Note, tactic.CreatedByUserId, []);
	}

	#endregion
}
