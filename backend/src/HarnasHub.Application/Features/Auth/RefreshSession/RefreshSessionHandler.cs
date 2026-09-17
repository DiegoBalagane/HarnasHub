using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Auth.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Auth.RefreshSession;

/// <summary>Handles <see cref="RefreshSessionCommand"/> by re-reading the caller's role from the database and issuing a new token for it.</summary>
public class RefreshSessionHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<RefreshSessionCommand, ErrorOr<AuthResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AuthResultDto>> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
	{
		var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken);

		if (user is null)
		{
			return AuthErrors.UserNotFound;
		}

		var token = jwtTokenGenerator.GenerateToken(user);

		return new AuthResultDto(token, user.Id, user.DisplayName, user.AccessLevel.ToString(), user.AvatarUrl);
	}

	#endregion
}
