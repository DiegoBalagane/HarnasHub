using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Auth.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.Auth.Register;

/// <summary>Handles <see cref="RegisterCommand"/> by creating the user and issuing an access token.</summary>
public class RegisterHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    ILogger<RegisterHandler> logger) : IRequestHandler<RegisterCommand, ErrorOr<AuthResultDto>>
{
    #region Public Methods

    public async Task<ErrorOr<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailTaken = await dbContext.Users
                .AnyAsync(u => u.Email == request.Email, cancellationToken);

            if (emailTaken)
            {
                return AuthErrors.EmailAlreadyRegistered;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = passwordHasher.Hash(request.Password),
                Role = request.Role,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            var token = jwtTokenGenerator.GenerateToken(user);

            return new AuthResultDto(token, user.Id, user.DisplayName, user.Role.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Nie udało się zarejestrować użytkownika {Email}", request.Email);
            return Error.Failure("Auth.RegisterFailed", "Nie udało się utworzyć konta.");
        }
    }

    #endregion
}
