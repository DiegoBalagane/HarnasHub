using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Tasks.AssignTask;
using HarnasHub.Application.Features.Tasks.CompleteTask;
using HarnasHub.Application.Features.Tasks.GetMyTasks;
using MediatR;

namespace HarnasHub.Api.Endpoints.Tasks;

/// <summary>Task assignment endpoints under /api/tasks.</summary>
public class TasksEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/tasks").WithTags("Tasks").RequireAuthorization(AuthorizationPolicies.TeamMember);

		group.MapGet("/mine", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetMyTasksQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		});

		group.MapPost("/", async (AssignTaskRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AssignTaskCommand(
				request.Title, request.Description, request.AssignedToUserId, request.DueAtUtc, request.TrainingMaterialId);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/{taskId:guid}/complete", async (Guid taskId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new CompleteTaskCommand(taskId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});
	}

	#endregion
}

/// <summary>Request body for POST /api/tasks.</summary>
public record AssignTaskRequest(
	string Title, string? Description, Guid AssignedToUserId, DateTime? DueAtUtc, Guid? TrainingMaterialId);
