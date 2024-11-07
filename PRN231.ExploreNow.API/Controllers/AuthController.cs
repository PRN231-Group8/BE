using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;
using PRN231.ExploreNow.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PRN231.ExploreNow.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
	private readonly IAuthService _authService;
	private readonly IUserService _userService;

	public AuthController(IAuthService authService, IUserService userService)
	{
		_authService = authService;
		_userService = userService;
	}


	[HttpPost]
	[Route("seed-roles")]
	public async Task<IActionResult> SeedRoles()
	{
		var seedRoles = await _authService.SeedRolesAsync();

		return Ok(seedRoles);
	}
	// Route -> Register
	[HttpPost]
	[Route("register")]
	public async Task<IActionResult> Register([FromBody][Required] RegisterResponse registerResponse)
	{
		if (!ModelState.IsValid)
			return BadRequest(new BaseResponse<object>
			{ IsSucceed = false, Message = "Invalid model state", Result = null });

		var authServiceResponse = await _authService.RegisterAsync(registerResponse);

		if (authServiceResponse.IsSucceed)
			return Ok(new BaseResponse<object>
			{
				IsSucceed = true,
				Message = "Account created successfully and check your email to verify account!",
				Result = null
			});
		return BadRequest(new BaseResponse<object>
		{ IsSucceed = false, Message = authServiceResponse.Token, Result = null });
	}

	// Route -> Login
	[HttpPost]
	[Route("login")]
	public async Task<IActionResult> Login([FromBody] LoginResponse loginResponse)
	{
		var loginResult = await _authService.LoginAsync(loginResponse);

		if (loginResult.IsSucceed)
			return Ok(loginResult);

		return BadRequest(new { message = "Username or password is incorrect" });
	}

	//verify
	[HttpGet]
	[Route("verify-email")]
	public async Task<IActionResult> VerifyEmail(string email, string token)
	{
		try
		{
			var result = await _userService.VerifyEmailTokenAsync(email, token);

			if (result)
				return Ok(new BaseResponse<object>
				{ IsSucceed = true, Message = "Email verified successfully.", Result = null });
			return BadRequest(new BaseResponse<object>
			{ IsSucceed = false, Message = "Invalid or expired token.", Result = null });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new BaseResponse<object> { IsSucceed = false, Message = ex.Message, Result = null });
		}
	}
	// Route -> make customer -> admin
	[HttpPost]
	[Route("make-admin")]
	public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionResponse updatePermissionResponse)
	{
		var operationResult = await _authService.MakeAdminAsync(updatePermissionResponse);

		if (operationResult.IsSucceed)
			return Ok(operationResult);

		return BadRequest(operationResult);
	}

	// Route -> make customer -> staff
	[HttpPost]
	[Route("make-staff")]
	public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionResponse updatePermissionResponse)
	{
		var operationResult = await _authService.MakeModeratorAsync(updatePermissionResponse);

		if (operationResult.IsSucceed)
			return Ok(operationResult);

		return BadRequest(operationResult);
	}

	// Login Google
	[HttpPost("google")]
	public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthRequest requestAuth)
	{
		var result = await _authService.HandleExternalLogin(requestAuth);
		if (!result.IsSucceed)
		{
			return BadRequest(new ExternalAuthResponse
			{
				IsSucceed = false,
				ErrorMessage = result.ErrorMessage,
				Token = null,
				Role = null,
				UserId = null,
				Email = null
			});
		}	

		return Ok(result);
	}
}