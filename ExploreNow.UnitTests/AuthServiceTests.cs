using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response.Auth;
using PRN231.ExploreNow.BusinessObject.Utilities;
using PRN231.ExploreNow.Services.Services;
using Xunit;

namespace PRN231.ExploreNow.UnitTests
{
	public class AuthServiceTests
	{
		private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
		private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
		private readonly Mock<ILogger<AuthService>> _mockLogger;
		private readonly Mock<IConfiguration> _mockConfiguration;
		private readonly Mock<IEmailVerify> _mockEmailVerify;
		private readonly AuthService _authService;
		private readonly Func<string> _originalCreateRandomTokenDelegate;

		public AuthServiceTests()
		{
			// Save original delegate token
			_originalCreateRandomTokenDelegate = TokenGenerator.CreateRandomTokenDelegate;

			var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
			_mockUserManager = new Mock<UserManager<ApplicationUser>>(mockUserStore.Object, null, null, null, null, null, null, null, null);

			var mockRoleStore = new Mock<IRoleStore<IdentityRole>>();
			_mockRoleManager = new Mock<RoleManager<IdentityRole>>(mockRoleStore.Object, null, null, null, null);

			_mockLogger = new Mock<ILogger<AuthService>>();
			_mockConfiguration = new Mock<IConfiguration>();

			// Mock the SMTP configuration
			_mockConfiguration.SetupGet(x => x["SMTP:Username"]).Returns("test@example.com");
			_mockConfiguration.SetupGet(x => x["SMTP:Password"]).Returns("password123");
			_mockConfiguration.SetupGet(x => x["SMTP:Host"]).Returns("smtp.example.com");
			_mockConfiguration.SetupGet(x => x["SMTP:Port"]).Returns("587");
			_mockConfiguration.SetupGet(x => x["SMTP:Secret"]).Returns("mock-secret");
			_mockConfiguration.SetupGet(x => x["SMTP:expiryInMinutes"]).Returns("5");

			// Mock EmailVerify
			_mockEmailVerify = new Mock<IEmailVerify>();

			// Mock configuration settings for JWT and GoogleAuthSettings
			var mockJwtSection = new Mock<IConfigurationSection>();
			mockJwtSection.Setup(x => x["Secret"]).Returns("super_secret_key");
			_mockConfiguration.Setup(x => x.GetSection("JWT")).Returns(mockJwtSection.Object);

			// Inject into AuthService
			_authService = new AuthService(
				_mockUserManager.Object,
				_mockRoleManager.Object,
				_mockLogger.Object,
				_mockConfiguration.Object,
				_mockEmailVerify.Object,
				new TokenGenerator()
			);
		}

		[Fact]
		public async Task SeedRolesAsync_ShouldReturn_Success_WhenRolesAreSeeded()
		{
			// Arrange
			_mockRoleManager.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

			// Act
			var result = await _authService.SeedRolesAsync();

			// Assert
			object value = result.IsSucceed.Should().BeTrue();
			result.Token.Should().Be("Role Seeding Done Successfully");

			_mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Exactly(3));
		}

		[Fact]
		public async Task RegisterAsync_ShouldReturn_Error_WhenUserNameAlreadyExists()
		{
			// Arrange
			var registerRequest = new RegisterResponse
			{
				UserName = "testUser",
				Email = "test@test.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!"
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(registerRequest.UserName))
				.ReturnsAsync(new ApplicationUser { UserName = registerRequest.UserName });

			// Act
			var result = await _authService.RegisterAsync(registerRequest);

			// Assert
			result.IsSucceed.Should().BeFalse();
			result.Token.Should().Be("UserName Already Exists");
		}

		[Fact]
		public async Task RegisterAsync_ShouldReturn_Error_WhenEmailAlreadyExists()
		{
			// Arrange
			var registerRequest = new RegisterResponse
			{
				UserName = "testUser",
				Email = "test@test.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!"
			};

			_mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
				.ReturnsAsync(new ApplicationUser { Email = registerRequest.Email });

			// Act
			var result = await _authService.RegisterAsync(registerRequest);

			// Assert
			result.IsSucceed.Should().BeFalse();
			result.Token.Should().Be("Email Already Exists");
		}

		public void Dispose()
		{
			TokenGenerator.CreateRandomTokenDelegate = _originalCreateRandomTokenDelegate;
		}

		[Fact]
		public async Task RegisterAsync_ShouldReturn_Success_WhenUserIsRegistered()
		{
			// Arrange
			TokenGenerator.CreateRandomTokenDelegate = () => "mocked-verification-token";

			var registerRequest = new RegisterResponse
			{
				UserName = "newUser",
				Email = "newUser@test.com",
				Password = "Password123!",
				ConfirmPassword = "Password123!",
				FirstName = "New",
				LastName = "User"
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(registerRequest.UserName))
				.ReturnsAsync((ApplicationUser)null);
			_mockUserManager.Setup(x => x.FindByEmailAsync(registerRequest.Email))
				.ReturnsAsync((ApplicationUser)null);
			_mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);
			_mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
				.ReturnsAsync(IdentityResult.Success);
			_mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
				.ReturnsAsync(IdentityResult.Success);

			_mockConfiguration.Setup(x => x["ValidAudience"]).Returns("http://localhost:3000");

			_mockEmailVerify.Setup(x => x.SendVerifyAccountEmail(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);

			// Act
			var result = await _authService.RegisterAsync(registerRequest);

			// Assert
			result.IsSucceed.Should().BeTrue();
			result.Token.Should().Be("Account created successfully and check your email to verify account!");

			// Verify that CreateAsync was called with the correct parameters
			_mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
				u.UserName == registerRequest.UserName &&
				u.Email == registerRequest.Email &&
				u.FirstName == registerRequest.FirstName &&
				u.LastName == registerRequest.LastName &&
				u.VerifyToken == "mocked-verification-token"
			), registerRequest.Password), Times.Once);

			// Verify that AddToRoleAsync was called
			_mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "CUSTOMER"), Times.Once);

			// Verify that UpdateAsync was called
			_mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);

			// Verify that SendVerifyAccountEmail was called
			_mockEmailVerify.Verify(x => x.SendVerifyAccountEmail(registerRequest.Email, "mocked-verification-token"), Times.Once);
		}

		[Fact]
		public async Task LoginAsync_ShouldReturn_Error_WhenInvalidCredentials()
		{
			// Arrange
			var loginRequest = new LoginResponse
			{
				UserName = "nonExistingUser",
				Password = "Password123!"
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(loginRequest.UserName))
				.ReturnsAsync((ApplicationUser)null);

			// Act
			var result = await _authService.LoginAsync(loginRequest);

			// Assert
			result.IsSucceed.Should().BeFalse();
			result.Token.Should().Be("Invalid Credentials");
		}

		[Fact]
		public async Task LoginAsync_ShouldReturn_Success_WhenValidCredentials()
		{
			// Arrange
			var loginRequest = new LoginResponse
			{
				UserName = "existingUser",
				Password = "Password123!"
			};

			var existingUser = new ApplicationUser
			{
				Id = "testUserId",
				UserName = loginRequest.UserName,
				Email = "existingUser@test.com",
				isActived = true,
				FirstName = "John",
				LastName = "Doe"
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(loginRequest.UserName))
				.ReturnsAsync(existingUser);

			_mockUserManager.Setup(x => x.CheckPasswordAsync(existingUser, loginRequest.Password))
				.ReturnsAsync(true);

			_mockUserManager.Setup(x => x.GetRolesAsync(existingUser))
				.ReturnsAsync(new List<string> { "CUSTOMER" });

			// Mock JWT settings
			_mockConfiguration.Setup(x => x["JWT:Secret"]).Returns("aaaaabDDDejExploreNowDSecretKeysnmaasekE");
			_mockConfiguration.Setup(x => x["JWT:ValidIssuer"]).Returns("https://localhost:7130");
			_mockConfiguration.Setup(x => x["JWT:ValidAudience"]).Returns("http://localhost:3000");

			// Act
			var result = await _authService.LoginAsync(loginRequest);

			// Assert
			result.IsSucceed.Should().BeTrue();
			result.Token.Should().NotBeNullOrEmpty();
			result.Role.Should().Be("CUSTOMER");
		}

		[Fact]
		public async Task MakeAdminAsync_ShouldReturn_Error_WhenUserDoesNotExist()
		{
			// Arrange
			var updatePermissionRequest = new UpdatePermissionResponse
			{
				UserName = "nonExistingUser"
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(updatePermissionRequest.UserName))
				.ReturnsAsync((ApplicationUser)null);

			// Act
			var result = await _authService.MakeAdminAsync(updatePermissionRequest);

			// Assert
			result.IsSucceed.Should().BeFalse();
			result.Token.Should().Be("Invalid User name!!!!!!!!");
		}

		[Fact]
		public async Task MakeAdminAsync_ShouldReturn_Success_WhenUserIsMadeAdmin()
		{
			// Arrange
			var updatePermissionRequest = new UpdatePermissionResponse
			{
				UserName = "existingUser"
			};

			var existingUser = new ApplicationUser
			{
				UserName = updatePermissionRequest.UserName
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(updatePermissionRequest.UserName))
				.ReturnsAsync(existingUser);

			_mockUserManager.Setup(x => x.GetRolesAsync(existingUser))
				.ReturnsAsync(new List<string> { "CUSTOMER" });

			_mockUserManager.Setup(x => x.RemoveFromRolesAsync(existingUser, It.IsAny<string[]>()))
				.ReturnsAsync(IdentityResult.Success);

			_mockUserManager.Setup(x => x.AddToRoleAsync(existingUser, StaticUserRoles.ADMIN))
				.ReturnsAsync(IdentityResult.Success);

			// Act
			var result = await _authService.MakeAdminAsync(updatePermissionRequest);

			// Assert
			result.IsSucceed.Should().BeTrue();
			result.Token.Should().Be("User is now an ADMIN");

			_mockUserManager.Verify(x => x.AddToRoleAsync(existingUser, StaticUserRoles.ADMIN), Times.Once);
		}

		[Fact]
		public async Task MakeModeratorAsync_ShouldReturn_Error_WhenUserDoesNotExist()
		{
			// Arrange
			var updatePermissionRequest = new UpdatePermissionResponse
			{
				UserName = "nonExistingUser"
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(updatePermissionRequest.UserName))
				.ReturnsAsync((ApplicationUser)null);

			// Act
			var result = await _authService.MakeModeratorAsync(updatePermissionRequest);

			// Assert
			result.IsSucceed.Should().BeFalse();
			result.Token.Should().Be("Invalid User name!!!!!!!!");
		}

		[Fact]
		public async Task MakeModeratorAsync_ShouldReturn_Success_WhenUserIsMadeModerator()
		{
			// Arrange
			var updatePermissionRequest = new UpdatePermissionResponse
			{
				UserName = "existingUser"
			};

			var existingUser = new ApplicationUser
			{
				UserName = updatePermissionRequest.UserName
			};

			_mockUserManager.Setup(x => x.FindByNameAsync(updatePermissionRequest.UserName))
				.ReturnsAsync(existingUser);

			_mockUserManager.Setup(x => x.GetRolesAsync(existingUser))
				.ReturnsAsync(new List<string> { "CUSTOMER" });

			_mockUserManager.Setup(x => x.RemoveFromRolesAsync(existingUser, It.IsAny<string[]>()))
				.ReturnsAsync(IdentityResult.Success);

			_mockUserManager.Setup(x => x.AddToRoleAsync(existingUser, StaticUserRoles.MODERATOR))
				.ReturnsAsync(IdentityResult.Success);

			// Act
			var result = await _authService.MakeModeratorAsync(updatePermissionRequest);

			// Assert
			result.IsSucceed.Should().BeTrue();
			result.Token.Should().Be("User is now an STAFF");

			_mockUserManager.Verify(x => x.AddToRoleAsync(existingUser, StaticUserRoles.MODERATOR), Times.Once);
		}
	}
}
