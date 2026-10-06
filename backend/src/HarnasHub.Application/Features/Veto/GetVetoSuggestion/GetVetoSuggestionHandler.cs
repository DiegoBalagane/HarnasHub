using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.Veto.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Veto.GetVetoSuggestion;

/// <summary>Handles <see cref="GetVetoSuggestionQuery"/> by gathering the per-map inputs and handing them to <see cref="VetoScoring"/>;
/// when an opponent report snapshot exists, its FACEIT advantage per map is fed in as an extra input.</summary>
public class GetVetoSuggestionHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetVetoSuggestionQuery, ErrorOr<VetoSuggestionDto>>
{
	#region Public Methods

	public async Task<ErrorOr<VetoSuggestionDto>> Handle(GetVetoSuggestionQuery request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.OpponentName);

		var data = await VetoInputLoader.LoadAsync(dbContext, key, cancellationToken);
		var report = await OpponentReportSnapshots.LoadAsync(dbContext, key, cancellationToken);
		var inputs = report is null ? data.Inputs : OpponentReportSnapshots.ApplyFaceitAdvantage(data.Inputs, report);

		return new VetoSuggestionDto(
			request.OpponentName.Trim(),
			VetoScoring.Suggest(inputs),
			data.Tendencies,
			data.RecordedOpponentVetoes);
	}

	#endregion
}
