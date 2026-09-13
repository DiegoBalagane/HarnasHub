namespace HarnasHub.Application.Features.TrainingMaterials.Shared;

/// <summary>A shared training resource.</summary>
public record TrainingMaterialDto(Guid Id, string Title, string Url, string? Category, string? Description);
