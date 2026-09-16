using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Nades.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Nades.GetNades;

/// <summary>Handles <see cref="GetNadesQuery"/>.</summary>
public class GetNadesHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetNadesQuery, ErrorOr<List<NadeEntryDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<NadeEntryDto>>> Handle(GetNadesQuery request, CancellationToken cancellationToken)
	{
		var query = dbContext.NadeEntries.AsQueryable();

		if (request.MapName is not null)
		{
			query = query.Where(n => n.MapName == request.MapName);
		}

		if (request.Type is not null)
		{
			query = query.Where(n => n.Type == request.Type);
		}

		return await query
			.OrderBy(n => n.MapName).ThenBy(n => n.Type).ThenBy(n => n.Title)
			.Select(n => new NadeEntryDto(n.Id, n.MapName, n.Type.ToString(), n.Title, n.Description, n.YoutubeUrl, n.CreatedByUserId))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
