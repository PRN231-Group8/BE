using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;
using PRN231.ExploreNow.BusinessObject.Utilities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PRN231.ExploreNow.Services.Services;

public class AuthService : IAuthService
{
	private readonly IConfiguration _configuration;
	private readonly IEmailVerify _emailVerify;
	private readonly IConfigurationSection _googleSettings;
	private readonly IConfigurationSection _jwtSettings;
	private readonly ILogger<AuthService> _logger;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ApplicationDbContext _context;

	public AuthService(
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager,
		ILogger<AuthService> logger,
		IConfiguration configuration,
		IEmailVerify emailVerify,
		TokenGenerator tokenGenerator,
		ApplicationDbContext context
	)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_configuration = configuration;
		_jwtSettings = _configuration.GetSection("JWT");
		_googleSettings = _configuration.GetSection("GoogleAuthSettings:Google");
		_logger = logger;
		_emailVerify = emailVerify;
		_context = context;
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

	public async Task<AuthResponse> RegisterAsync(RegisterResponse registerResponse)
	{
		var isExistsUser = await _userManager.FindByNameAsync(registerResponse.UserName);

		if (isExistsUser != null)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "UserName Already Exists"
			};

		// Check if email is already in use
		var isExistsEmail = await _userManager.FindByEmailAsync(registerResponse.Email);
		if (isExistsEmail != null)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Email Already Exists"
			};

		if (registerResponse.Password != registerResponse.ConfirmPassword)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "The password and confirmation password do not match."
			};

		var newUser = new ApplicationUser
		{
			Id = Guid.NewGuid().ToString(),
			FirstName = registerResponse.FirstName,
			LastName = registerResponse.LastName,
			Email = registerResponse.Email,
			UserName = registerResponse.UserName,
			SecurityStamp = Guid.NewGuid().ToString(),
			VerifyTokenExpires = DateTime.Now.AddHours(24),
			CreatedDate = DateTime.UtcNow,
		};

		var createUserResult = await _userManager.CreateAsync(newUser, registerResponse.Password);

		if (!createUserResult.Succeeded)
		{
			var errorString = "User Creation Failed Because: " +
							  string.Join(" # ", createUserResult.Errors.Select(e => e.Description));
			return new AuthResponse { IsSucceed = false, Token = errorString };
		}

		// Add a Default USER Role to all users
		await _userManager.AddToRoleAsync(newUser, StaticUserRoles.CUSTOMER);

		// Generate verification token using custom TokenGenerator
		var verificationToken = TokenGenerator.CreateRandomToken();
		newUser.VerifyToken = verificationToken;

		// Update user with verification token
		var updateUserResult = await _userManager.UpdateAsync(newUser);
		if (!updateUserResult.Succeeded)
		{
			var errorString = "User Update Failed Because: " +
							  string.Join(" # ", updateUserResult.Errors.Select(e => e.Description));
			return new AuthResponse { IsSucceed = false, Token = errorString };
		}

		// Send verification email
		var emailSent = _emailVerify.SendVerifyAccountEmail(newUser.Email, verificationToken);
		if (!emailSent)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Email sending failed!"
			};

		return new AuthResponse
		{
			IsSucceed = true,
			Token = "Account created successfully and check your email to verify account!"
		};
	}

	public async Task<AuthResponse> LoginAsync(LoginResponse loginResponse)
	{
		var user = await _userManager.FindByNameAsync(loginResponse.UserName);

		if (user is null)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Invalid Credentials"
			};
		if (!user.isActived)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Account not verified!"
			};

		var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginResponse.Password);

		if (!isPasswordCorrect)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Invalid Credentials"
			};

		user.DeviceId = loginResponse.DeviceId;
		await _userManager.UpdateAsync(user);

		var userRoles = await _userManager.GetRolesAsync(user);
		var role = userRoles.FirstOrDefault() ?? StaticUserRoles.CUSTOMER;

		var authClaims = new List<Claim>
		{
			new(ClaimTypes.Name, user.UserName),
			new(ClaimTypes.NameIdentifier, user.Id),
			new("JWTID", Guid.NewGuid().ToString()),
			new("FirstName", user.FirstName),
			new("LastName", user.LastName),
			new("email", user.Email)
		};

		if (!string.IsNullOrEmpty(loginResponse.DeviceId))
		{
			authClaims.Add(new Claim("deviceId", loginResponse.DeviceId));
		}

		foreach (var userRole in userRoles) authClaims.Add(new Claim(ClaimTypes.Role, userRole));

		var token = GenerateNewJsonWebToken(authClaims);

		await _context.SaveChangesAsync();

		return new AuthResponse { IsSucceed = true, Token = token, Role = role, UserId = user.Id, Email = user.Email, DeviceId = loginResponse.DeviceId };
	}

	private string GenerateNewJsonWebToken(List<Claim> claims)
	{
		var authSecret = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
		);
		var expires = DateTime.UtcNow.AddDays(7);

		var tokenObject = new JwtSecurityToken(
			_configuration["JWT:ValidIssuer"],
			_configuration["JWT:ValidAudience"],
			expires: DateTime.Now.AddHours(1),
			claims: claims,
			signingCredentials: new SigningCredentials(
				authSecret,
				SecurityAlgorithms.HmacSha256
			)
		);

		var token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

		return token;
	}

	private SigningCredentials GetSigningCredentials()
	{
		var key = Encoding.UTF8.GetBytes(_jwtSettings.GetSection("Secret").Value);
		var secret = new SymmetricSecurityKey(key);

		return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
	}

	private async Task<List<Claim>> GetClaims(ApplicationUser user)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.Name, user.Email)
		};

		var roles = await _userManager.GetRolesAsync(user);
		foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

		return claims;
	}

	private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
	{
		var tokenOptions = new JwtSecurityToken(
			_jwtSettings["ValidIssuer"],
			_jwtSettings["ValidAudience"],
			claims,
			expires: DateTime.Now.AddMinutes(Convert.ToDouble(_jwtSettings["expiryInMinutes"])),
			signingCredentials: signingCredentials);

		return tokenOptions;
	}

    public async Task<string> GenerateToken(ApplicationUser user, bool isExternalLogin = false)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = await GetClaims(user);

        // Add additional claims for external login if required
        if (isExternalLogin)
        {
            claims.Add(new Claim("FirstName", user.FirstName ?? string.Empty));
            claims.Add(new Claim("LastName", user.LastName ?? string.Empty));
            claims.Add(new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty));
            claims.Add(new Claim(ClaimTypes.Email, user.Email ?? string.Empty));
        }

        var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return token;
    }

    public async Task<AuthResponse> MakeAdminAsync(
		UpdatePermissionResponse updatePermissionDto
	)
	{
		var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

		if (user is null)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Invalid User name!!!!!!!!"
			};
		var roles = await _userManager.GetRolesAsync(user);
		await _userManager.RemoveFromRolesAsync(user, roles.ToArray());

		await _userManager.AddToRoleAsync(user, StaticUserRoles.ADMIN);

		return new AuthResponse
		{
			IsSucceed = true,
			Token = "User is now an ADMIN"
		};
	}

	public async Task<AuthResponse> MakeModeratorAsync(
		UpdatePermissionResponse updatePermissionDto
	)
	{
		var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

		if (user is null)
			return new AuthResponse
			{
				IsSucceed = false,
				Token = "Invalid User name!!!!!!!!"
			};
		var roles = await _userManager.GetRolesAsync(user);
		await _userManager.RemoveFromRolesAsync(user, roles.ToArray());

		await _userManager.AddToRoleAsync(user, StaticUserRoles.MODERATOR);

		return new AuthResponse
		{
			IsSucceed = true,
			Token = "User is now an STAFF"
		};
	}

	public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthRequest externalAuth)
	{
		try
		{
			var settings = new GoogleJsonWebSignature.ValidationSettings()
			{
				Audience = new List<string>() { _googleSettings.GetSection("ClientId").Value }
			};
			var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
			return payload;
		}
		catch (Exception ex)
		{
			//log an exception
			return null;
		}
	}

	public async Task<ExternalAuthResponse> HandleExternalLogin(ExternalAuthRequest externalAuth)
	{
		var payload = await VerifyGoogleToken(externalAuth);
		if (payload == null)
			return new ExternalAuthResponse { IsSucceed = false, ErrorMessage = "Invalid External Authentication." };

		var info = new UserLoginInfo(externalAuth.Provider, payload.Subject, externalAuth.Provider);

		var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

		user = await _userManager.FindByEmailAsync(payload.Email);

		if (user == null)
		{
			user = new ApplicationUser
			{
				Id = Guid.NewGuid().ToString(),
				Email = payload.Email,
				UserName = payload.Email,
				FirstName = payload.GivenName,
				LastName = payload.FamilyName,
				isActived = true,
				CreatedDate = DateTime.UtcNow,
				EmailConfirmed = true
			};

			var result = await _userManager.CreateAsync(user);
			if (!result.Succeeded)
			{
				return new ExternalAuthResponse { IsSucceed = false, ErrorMessage = "Failed to create user account." };
			}
			await _userManager.AddToRoleAsync(user, StaticUserRoles.CUSTOMER);
		}

		var logins = await _userManager.GetLoginsAsync(user);
		if (!logins.Any(l => l.LoginProvider == externalAuth.Provider && l.ProviderKey == payload.Subject))
		{
			await _userManager.AddLoginAsync(user, info);
		}

        var token = await GenerateToken(user, isExternalLogin: true);
        return new ExternalAuthResponse
		{
			Token = token,
			IsSucceed = true,
			Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
			UserId = user.Id,
			Email = user.Email
		};
	}
}