using ExploreNow.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExploreNow.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [Route("seed-roles")]
    public async Task<IActionResult> SeedRoles()
    {
        var seedRoles = await _authService.SeedRolesAsync();

        return Ok(seedRoles);
    }
}