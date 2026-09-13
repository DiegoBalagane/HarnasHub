using HarnasHub.Application.Abstractions;

namespace HarnasHub.Infrastructure.Security;

/// <summary>BCrypt-based implementation of <see cref="IPasswordHasher"/>.</summary>
public class PasswordHasher : IPasswordHasher
{
    #region Public Methods

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);

    #endregion
}
