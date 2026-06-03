using WorkflowHub.Application.Auth.Models;
using WorkflowHub.Data.Entities;

namespace WorkflowHub.Application.Auth;

public static class AuthUserMapper
{
    public static AuthUserDto ToDto(User user) =>
        new(
            user.Id,
            user.Email,
            user.Name,
            user.DisplayName,
            user.Username,
            user.Role,
            user.Team,
            user.Bio,
            user.AvatarUrl,
            user.CreatedAtUtc);
}
