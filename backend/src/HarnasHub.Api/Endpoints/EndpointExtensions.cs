using System.Reflection;

namespace HarnasHub.Api.Endpoints;

/// <summary>Discovers and maps every <see cref="IEndpoint"/> implementation in this assembly.</summary>
public static class EndpointExtensions
{
	#region Public Methods

	public static void MapAllEndpoints(this IEndpointRouteBuilder app)
	{
		var endpointTypes = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IEndpoint).IsAssignableFrom(type));

		foreach (var endpointType in endpointTypes)
		{
			endpointType.GetMethod(nameof(IEndpoint.MapEndpoints))!.Invoke(null, [app]);
		}
	}

	#endregion
}
