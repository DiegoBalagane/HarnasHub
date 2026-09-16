namespace HarnasHub.Api.Endpoints;

/// <summary>Contract for a group of related Minimal API endpoints.</summary>
public interface IEndpoint
{
	static abstract void MapEndpoints(IEndpointRouteBuilder app);
}
