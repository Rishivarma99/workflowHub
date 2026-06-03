using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Application.Auth.Options;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Auth.Services;

public interface IRefreshTokenService
{
    Task<(string PlainToken, RefreshToken Entity)> CreateAsync(
        User user,
        string? deviceInfo,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> FindActiveByPlainTokenAsync(
        string plainToken,
        CancellationToken cancellationToken = default);

    void Revoke(RefreshToken token);

    void MarkUsed(RefreshToken token);
}

public sealed class RefreshTokenService(
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<JwtOptions> options) : IRefreshTokenService
{
    public Task<(string PlainToken, RefreshToken Entity)> CreateAsync(
        User user,
        string? deviceInfo,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var plain = GeneratePlainToken();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(plain),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(options.Value.RefreshTokenDays),
            DeviceInfo = deviceInfo
        };

        refreshTokenRepository.Add(entity);
        return Task.FromResult((plain, entity));
    }

    public Task<RefreshToken?> FindActiveByPlainTokenAsync(
        string plainToken,
        CancellationToken cancellationToken = default) =>
        refreshTokenRepository.GetActiveByTokenHashWithUserAsync(Hash(plainToken), cancellationToken);

    public void Revoke(RefreshToken token) => token.RevokedAtUtc = DateTime.UtcNow;

    public void MarkUsed(RefreshToken token) => token.IsUsed = true;

    private static string GeneratePlainToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string Hash(string plain)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexString(bytes);
    }
}
