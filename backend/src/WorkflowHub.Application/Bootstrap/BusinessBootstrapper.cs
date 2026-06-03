using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Application.Auth.Options;
using WorkflowHub.Application.Auth.Services;
using WorkflowHub.Application.Contracts.Identity;
using WorkflowHub.Application.Contracts.Persistence;
using WorkflowHub.Application.Services.Identity;
using WorkflowHub.Application.Contracts.Repositories;
using WorkflowHub.Application.Persistence;
using WorkflowHub.Application.Persistence.Repositories;

namespace WorkflowHub.Application.Bootstrap;

public static class BusinessBootstrapper
{
    public static IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));

        var assembly = typeof(BusinessBootstrapper).Assembly;
        services.AddCqrs(assembly);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserIdentityRepository, UserIdentityRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IExternalIdentityVerifier, GoogleTokenVerifier>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<IJwtSecurityStampValidator, JwtSecurityStampValidator>();

        return services;
    }
}
