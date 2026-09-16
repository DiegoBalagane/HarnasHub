using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Auth.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Auth.DiscordLogin;

/// <summary>Handles <see cref="DiscordLoginCommand"/>.</summary>
public class DiscordLoginHandler(
	IApplicationDbContext dbContext,
	IDiscordOAuthClient discordOAuthClient,
	IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<DiscordLoginCommand, ErrorOr<AuthResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AuthResultDto>> Handle(DiscordLoginCommand request, CancellationToken cancellationToken)
	{
		var profile = await discordOAuthClient.ExchangeCodeAsync(request.Code, cancellationToken);

		if (profile is null)
		{
			return AuthErrors.DiscordExchangeFailed;
		}

		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.DiscordId == profile.DiscordId, cancellationToken);

		if (user is null)
		{
			// Everyone starts as Guest with no access to team data — a Manager promotes people from /roster.
			user = new User
			{
				Id = Guid.NewGuid(),
				DiscordId = profile.DiscordId,
				DisplayName = profile.Username,
				AvatarUrl = profile.AvatarUrl,
				Role = UserRole.Guest,
				CreatedAtUtc = DateTime.UtcNow
			};

			dbContext.Users.Add(user);
		}
		else
		{
			// Keep the locally-cached Discord name/avatar in sync in case the player changed them.
			user.DisplayName = profile.Username;
			user.AvatarUrl = profile.AvatarUrl;
		}

		await dbContext.SaveChangesAsync(cancellationToken);

		var token = jwtTokenGenerator.GenerateToken(user);

		return new AuthResultDto(token, user.Id, user.DisplayName, user.Role.ToString(), user.AvatarUrl);
	}

	#endregion
}
