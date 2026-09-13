using ErrorOr;
using HarnasHub.Application.Features.TrainingMaterials.Shared;
using MediatR;

namespace HarnasHub.Application.Features.TrainingMaterials.GetMaterials;

/// <summary>Returns every training material.</summary>
public record GetMaterialsQuery : IRequest<ErrorOr<List<TrainingMaterialDto>>>;
