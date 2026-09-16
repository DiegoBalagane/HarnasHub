using System.Reflection;
using FluentValidation;
using HarnasHub.Application.Common.Behaviors;
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

		return services;
	}

	#endregion
}
