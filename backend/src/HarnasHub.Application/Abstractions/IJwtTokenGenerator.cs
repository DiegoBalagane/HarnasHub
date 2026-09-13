using HarnasHub.Core.Entities;

namespace HarnasHub.Application.Abstractions;

/// <summary>Issues signed JWT access tokens for authenticated users.</summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
