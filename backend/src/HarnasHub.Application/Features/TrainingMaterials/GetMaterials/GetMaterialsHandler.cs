using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TrainingMaterials.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.TrainingMaterials.GetMaterials;

/// <summary>Handles <see cref="GetMaterialsQuery"/>.</summary>
public class GetMaterialsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMaterialsQuery, ErrorOr<List<TrainingMaterialDto>>>
{
    #region Public Methods

    public async Task<ErrorOr<List<TrainingMaterialDto>>> Handle(GetMaterialsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.TrainingMaterials
            .OrderByDescending(m => m.CreatedAtUtc)
            .Select(m => new TrainingMaterialDto(m.Id, m.Title, m.Url, m.Category, m.Description))
            .ToListAsync(cancellationToken);
    }

    #endregion
}
