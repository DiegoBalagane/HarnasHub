using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Jobs;

namespace HarnasHub.Api.Common;

/// <summary>Reads the authenticated user's id, access level, and coach tag from the JWT claims of the current request — or,
/// inside a background job (no HTTP context), the id of the user who started the job.</summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor, JobExecutionContext jobContext) : ICurrentUserService
{
	#region Public Properties

	public Guid UserId
	{
		get
		{
			if (httpContextAccessor.HttpContext is null && jobContext.RequestedByUserId is { } jobUserId)
			{
				return jobUserId;
			}

			var user = httpContextAccessor.HttpContext?.User;

			// JwtBearer may or may not remap "sub" to ClaimTypes.NameIdentifier depending on the token handler in use — check both.
			var claimValue = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
				?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (!Guid.TryParse(claimValue, out var userId))
			{
				throw new InvalidOperationException("Brak identyfikatora użytkownika w tokenie JWT.");
			}

			return userId;
		}
	}

	// The access-level claim is always written first by the token generator, and FindFirst preserves claim order,
	// so this keeps returning "Guest"/"Player"/"Manager" even when a trailing "Coach" role claim is present.
	public string Role => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

	public bool IsCoach => httpContextAccessor.HttpContext?.User.HasClaim(ClaimTypes.Role, "Coach") ?? false;

	#endregion
}
