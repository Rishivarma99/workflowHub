using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowHub.Application.Auth.Options;
using WorkflowHub.Application.Auth.Services;
using WorkflowHub.Application.Contracts.Identity;
using WorkflowHub.Application.Features.AgentAssets.Services;
using WorkflowHub.Application.Features.Discover.Services;
using WorkflowHub.Application.Features.Publish.Services;
using WorkflowHub.Application.Services.Identity;
using WorkflowHub.Data.Abstractions.Identity;

namespace WorkflowHub.Application.Bootstrap;

public static class BusinessBootstrapper
{
    public static IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));

        var assembly = typeof(BusinessBootstrapper).Assembly;
        services.AddCqrs(assembly);

        services.AddHttpClient<IGitHubRepositoryClient, GitHubRepositoryClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WorkflowHub/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        });

        services.AddScoped<IExternalIdentityVerifier, GoogleTokenVerifier>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddScoped<IJwtSecurityStampValidator, JwtSecurityStampValidator>();
        services.AddScoped<DiscoverWorkflowListService>();
        services.AddScoped<AgentAssetListService>();

        return services;
    }
}
