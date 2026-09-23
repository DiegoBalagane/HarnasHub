using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Tasks.ApproveTask;
using HarnasHub.Application.Features.Tasks.AssignTask;
using HarnasHub.Application.Features.Tasks.DeleteTask;
using HarnasHub.Application.Features.Tasks.GetAllTasks;
using HarnasHub.Application.Features.Tasks.GetMyTasks;
using HarnasHub.Application.Features.Tasks.RejectTask;
using HarnasHub.Application.Features.Tasks.SubmitTaskForReview;
using HarnasHub.Application.Features.Tasks.UpdateTask;
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

		group.MapGet("/all", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new GetAllTasksQuery(), cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/", async (AssignTaskRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new AssignTaskCommand(
				request.Title, request.Description, request.AssignedToUserId, request.DueAtUtc, request.TrainingMaterialId);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPatch("/{taskId:guid}", async (Guid taskId, UpdateTaskRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new UpdateTaskCommand(taskId, request.Title, request.Description, request.DueAtUtc, request.TrainingMaterialId);
			var result = await sender.Send(command, cancellationToken);
			return result.Match(success => Results.Ok(success), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapDelete("/{taskId:guid}", async (Guid taskId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new DeleteTaskCommand(taskId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/{taskId:guid}/submit", async (Guid taskId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new SubmitTaskForReviewCommand(taskId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		});

		group.MapPost("/{taskId:guid}/approve", async (Guid taskId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new ApproveTaskCommand(taskId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));

		group.MapPost("/{taskId:guid}/reject", async (Guid taskId, ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new RejectTaskCommand(taskId), cancellationToken);
			return result.Match(success => Results.NoContent(), errors => errors.ToProblemResult());
		}).RequireAuthorization(policy => policy.RequireRole("Coach", "Manager"));
	}

	#endregion
}

/// <summary>Request body for POST /api/tasks.</summary>
public record AssignTaskRequest(
	string Title, string? Description, Guid AssignedToUserId, DateTime? DueAtUtc, Guid? TrainingMaterialId);

/// <summary>Request body for PATCH /api/tasks/{taskId}.</summary>
public record UpdateTaskRequest(string Title, string? Description, DateTime? DueAtUtc, Guid? TrainingMaterialId);
