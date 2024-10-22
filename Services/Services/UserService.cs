using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.BackendMessages;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
	public class UserService : IUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private Cloudinary _cloudinary;
		private IConfiguration _configuration;
		private IConfigurationSection _cloundinarySetting;
		private IUnitOfWork @object;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;

		public UserService(IUnitOfWork unitOfWork, IConfiguration configuration, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
			_cloundinarySetting = _configuration.GetSection("CloudinarySetting");
			_userManager = userManager;
			_contextAccessor = contextAccessor;
		}

		public async Task<string> SaveImage(IFormFile file)
		{
			var cloud = new Account
			{
				Cloud = _configuration["CloudinarySetting:CloudName"],
				ApiKey = _configuration["CloudinarySetting:ApiKey"],
				ApiSecret = _configuration["CloudinarySetting:ApiSecret"]
			};
			_cloudinary = new Cloudinary(cloud);

			var uploadResult = new ImageUploadResult();
			if (file.Length > 0)
			{
				using var stream = file.OpenReadStream();
				var uploadParams = new ImageUploadParams
				{
					File = new FileDescription(file.FileName, stream),
					Transformation = new Transformation().Height(300).Width(200)
				};
				uploadResult = await _cloudinary.UploadAsync(uploadParams);

			}
			return uploadResult.SecureUrl.ToString();
		}

		public async Task<UserProfileResponseModel> UpdateUserProfile(string id, UserProfileRequestModel profile)
		{
			var user = MapToProfile(profile);
			user.Id = id;
			return await _unitOfWork.UserRepository.UpdateProfileAsync(user);
		}

		public ApplicationUser MapToProfile(UserProfileRequestModel user)
		{
			var currUser = _userManager.GetUserAsync(_contextAccessor.HttpContext.User).Result;
			var currUserName = currUser?.UserName ?? "Admin";
			return new ApplicationUser
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				Dob = user.Dob,
				Gender = user.Gender,
				AvatarPath = user.AvatarPath,
				LastUpdatedBy = currUserName,
				CreatedDate = DateTime.Now,
				LastUpdatedDate = DateTime.Now,
			};
		}

		public async Task<bool> VerifyEmailTokenAsync(string email, string token)
		{
			var userRepo = _unitOfWork.GetRepository<IUserRepository>();
			var user = await userRepo.GetUserByEmailAsync(email);

			if (user == null || user.VerifyToken != token || user.isActived)
			{
				return false;
			}

			user.isActived = true;
			user.VerifyToken = null;
			user.VerifyTokenExpires = DateTime.MinValue;

			userRepo.Update(user);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}
		public async Task<UserProfileResponseModel> GetUserByEmailAsync(string email)
		{
			var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
			if (user == null)
			{
				return null;
			}
			return new UserProfileResponseModel
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				Dob = user.Dob,
				Gender = user.Gender,
				AvatarPath = user.AvatarPath
			};
		}
	}
}
