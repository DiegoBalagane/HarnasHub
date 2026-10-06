using System.Reflection;
using FluentValidation;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Behaviors;
using HarnasHub.Application.Common.Faceit;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Application.Common.Notifications;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using Microsoft.Extensions.DependencyInjection;

namespace HarnasHub.Application;

/// <summary>Registers MediatR handlers, FluentValidation validators, and the validation pipeline behavior.</summary>
public static class DependencyInjection
{
	#region Public Methods

	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
		services.AddValidatorsFromAssembly(assembly);
		services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
		services.AddSingleton<RoundSignatureCache>();

		// Background jobs: the worker marks a scope as "running job X" and handlers report progress through IJobProgress
		// (a no-op outside a job). The FACEIT demo-name lookup is shared by the result and opponent demo analyses.
		services.AddScoped<JobExecutionContext>();
		services.AddScoped<IJobProgress, JobProgress>();
		services.AddScoped<FaceitDemoMatchLookup>();
		services.AddScoped<TeamNotifications>();

		return services;
	}

	#endregion
}
