using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TrainingMaterials.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.TrainingMaterials.AddMaterial;

/// <summary>Handles <see cref="AddMaterialCommand"/> by persisting the new training material.</summary>
public class AddMaterialHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    : IRequestHandler<AddMaterialCommand, ErrorOr<TrainingMaterialDto>>
{
    #region Public Methods

    public async Task<ErrorOr<TrainingMaterialDto>> Handle(AddMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = new TrainingMaterial
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Url = request.Url,
            Category = request.Category,
            Description = request.Description,
            CreatedByUserId = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.TrainingMaterials.Add(material);
        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.NotifyAsync("materials", cancellationToken);

        return new TrainingMaterialDto(material.Id, material.Title, material.Url, material.Category, material.Description);
    }

    #endregion
}
