using FinanCore.Application.DTOs.Auth;

namespace FinanCore.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
