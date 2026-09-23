using ErrorOr;
using HarnasHub.Application.Features.TrainingMaterials.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.TrainingMaterials.AddMaterial;

/// <summary>Adds a training material. Coach/Manager only — enforced at the endpoint.</summary>
public record AddMaterialCommand(
	string Title,
	string Url,
	MaterialCategory? Category,
	string? Description) : IRequest<ErrorOr<TrainingMaterialDto>>;
