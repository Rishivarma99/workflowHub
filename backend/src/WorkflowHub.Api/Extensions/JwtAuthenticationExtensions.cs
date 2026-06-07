using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WorkflowHub.Application.Auth.Options;
using WorkflowHub.Application.Auth.Services;

namespace WorkflowHub.Api.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        }

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub"
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        var sub = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? principal?.FindFirst("sub")?.Value;
                        var stamp = principal?.FindFirst("security_stamp")?.Value;
                        if (!Guid.TryParse(sub, out var userId) || string.IsNullOrEmpty(stamp))
                        {
                            context.Fail("Invalid token claims.");
                            return;
                        }

                        var validator = context.HttpContext.RequestServices
                            .GetRequiredService<IJwtSecurityStampValidator>();

                        if (!await validator.IsValidAsync(userId, stamp))
                        {
                            context.Fail("Token is no longer valid.");
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
