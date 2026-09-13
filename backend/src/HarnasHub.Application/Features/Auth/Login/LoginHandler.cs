using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Auth.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Auth.Login;

/// <summary>Handles <see cref="LoginQuery"/> by verifying credentials and issuing an access token.</summary>
public class LoginHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginQuery, ErrorOr<AuthResultDto>>
{
    #region Public Methods

    public async Task<ErrorOr<AuthResultDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return AuthErrors.InvalidCredentials;
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto(token, user.Id, user.DisplayName, user.Role.ToString());
    }

    #endregion
}
