using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HarnasHub.Infrastructure.Security;

/// <summary>Issues signed JWT access tokens using the configured <see cref="JwtSettings"/>.</summary>
public class JwtTokenGenerator(IOptions<JwtSettings> jwtSettings) : IJwtTokenGenerator
{
	#region Private Fields

	private readonly JwtSettings _jwtSettings = jwtSettings.Value;

	#endregion

	#region Public Methods

	public string GenerateToken(User user)
	{
		var claims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new(ClaimTypes.Name, user.DisplayName),
			new(ClaimTypes.Role, user.AccessLevel.ToString())
		};

		// The coach tag rides along as a second role claim: RequireRole/IsInRole OR-match across role claims, so
		// "Coach or Manager" policies keep working, while FindFirst(ClaimTypes.Role) still yields the access level
		// (added first above) for callers that read a single role string.
		if (user.IsCoach)
		{
			claims.Add(new Claim(ClaimTypes.Role, "Coach"));
		}

		if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
		{
			claims.Add(new Claim("avatar_url", user.AvatarUrl));
		}

		var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
		var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Issuer,
			audience: _jwtSettings.Audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	#endregion
}
