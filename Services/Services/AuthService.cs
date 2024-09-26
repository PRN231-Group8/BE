using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services;

public class AuthService : IAuthService

{
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> SeedRolesAsync()
    {
        var isOwnerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.MODERATOR);
        var isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
        var isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.CUSTOMER);

        if (isOwnerRoleExists && isAdminRoleExists && isUserRoleExists)
            return new AuthResponse
            {
                IsSucceed = true,
                Token = "Roles Seeding is Already Done"
            };

        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.CUSTOMER));
        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
        await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.MODERATOR));

        return new AuthResponse
        {
            IsSucceed = true,
            Token = "Role Seeding Done Successfully"
        };
    }
}