using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
	public class LocationService : ILocationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly Cloudinary _cloudinary;
		private readonly IConfiguration _configuration;
		private readonly IMapper _mapper;

		public LocationService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor,
			UserManager<ApplicationUser> userManager, IConfiguration configuration, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_contextAccessor = contextAccessor;
			_userManager = userManager;
			_configuration = configuration;
			_mapper = mapper;

			var account = new Account(
				_configuration["CloudinarySetting:CloudName"],
				_configuration["CloudinarySetting:ApiKey"],
				_configuration["CloudinarySetting:ApiSecret"]);
			_cloudinary = new Cloudinary(account);
		}

		public async Task<(List<LocationResponse> Items, int TotalCount)> GetAllLocationsAsync(int page, int pageSize, WeatherStatus? sortByStatus, string? searchTerm)
		{
			return await _unitOfWork.LocationRepository.GetAllLocationsAsync(page, pageSize, sortByStatus, searchTerm);
		}

		public async Task<LocationResponse> GetByIdAsync(Guid id)
		{
			var location = await _unitOfWork.GetRepositoryByEntity<Location>()
				.GetQueryable()
				.Where(l => l.Id == id && !l.IsDeleted)
				.Include(l => l.Photos)
				.FirstOrDefaultAsync();

			return _mapper.Map<LocationResponse>(location);
		}

		public async Task<LocationResponse> CreateAsync(LocationCreateRequest locationsRequest, List<IFormFile> files)
		{
			var location = _mapper.Map<Location>(locationsRequest);
			var currentUser = await GetAuthenticatedUserAsync();

			location.CreatedBy = currentUser.UserName;
			location.CreatedDate = DateTime.Now;
			location.LastUpdatedBy = currentUser.UserName;
			location.Code = GenerateUniqueCode();

			// Upload each photo and add to location
			if (files != null && files.Any())
			{
				foreach (var file in files)
				{
					var photoUrl = await SaveImage(file);
					location.Photos.Add(new Photo
					{
						Url = photoUrl,
						Alt = file.FileName,
						Code = GenerateUniqueCode(),
						CreatedBy = location.CreatedBy,
						CreatedDate = DateTime.Now,
						LastUpdatedBy = location.LastUpdatedBy
					});
				}
			}

			await _unitOfWork.GetRepositoryByEntity<Location>().AddAsync(location);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<LocationResponse>(location);
		}

		public async Task<LocationResponse> UpdateAsync(Guid id, LocationsRequest locationsRequest, List<IFormFile> files = null)
		{
			var currentUser = await GetAuthenticatedUserAsync();
			var existingLocation = await _unitOfWork.GetRepositoryByEntity<Location>()
				.GetQueryable()
				.Where(l => l.Id == id && !l.IsDeleted)
				.Include(l => l.Photos)
				.SingleOrDefaultAsync();

			if (existingLocation == null)
			{
				throw new Exception("Location not found.");
			}

			_mapper.Map(locationsRequest, existingLocation);
			existingLocation.LastUpdatedBy = currentUser.UserName;
			existingLocation.LastUpdatedDate = DateTime.Now;

			// Tạo list mới để lưu tất cả photos
			var updatedPhotos = new List<Photo>();

			// CASE 2: Có Photos trong request
			if (locationsRequest.Photos?.Any() == true)
			{
				foreach (var photoRequest in locationsRequest.Photos)
				{
					if (photoRequest.Id.HasValue)
					{
						// Tìm ảnh trong database hiện tại
						var existingPhoto = existingLocation.Photos.SingleOrDefault(p => p.Id == photoRequest.Id);
						if (existingPhoto != null)
						{
							existingPhoto.LastUpdatedBy = currentUser.UserName;
							existingPhoto.LastUpdatedDate = DateTime.Now;
							updatedPhotos.Add(existingPhoto);
						}
						else
						{
							updatedPhotos.Add(new Photo
							{
								Id = photoRequest.Id.Value,
								Url = photoRequest.Url,
								Alt = photoRequest.Alt,
								Code = GenerateUniqueCode(),
								CreatedBy = currentUser.UserName,
								CreatedDate = DateTime.Now,
								LastUpdatedBy = currentUser.UserName,
								LastUpdatedDate = DateTime.Now
							});
						}
					}
					else
					{
						updatedPhotos.Add(new Photo
						{
							Url = photoRequest.Url,
							Alt = photoRequest.Alt,
							Code = GenerateUniqueCode(),
							CreatedBy = currentUser.UserName,
							CreatedDate = DateTime.Now,
							LastUpdatedBy = currentUser.UserName,
							LastUpdatedDate = DateTime.Now
						});
					}
				}

				// Xử lý files mới (nếu có) trong CASE 2
				if (files?.Any() == true)
				{
					foreach (var file in files)
					{
						var photoUrl = await SaveImage(file);
						updatedPhotos.Add(new Photo
						{
							Url = photoUrl,
							Alt = file.FileName,
							Code = GenerateUniqueCode(),
							CreatedBy = currentUser.UserName,
							CreatedDate = DateTime.Now,
							LastUpdatedBy = currentUser.UserName,
							LastUpdatedDate = DateTime.Now
						});
					}
				}
			}
			// CASE 1: Chỉ có files mới
			else if (files?.Any() == true)
			{
				// Xử lý files mới nếu không có Photos trong request
				foreach (var file in files)
				{
					var photoUrl = await SaveImage(file);
					updatedPhotos.Add(new Photo
					{
						Url = photoUrl,
						Alt = file.FileName,
						Code = GenerateUniqueCode(),
						CreatedBy = currentUser.UserName,
						CreatedDate = DateTime.Now,
						LastUpdatedBy = currentUser.UserName,
						LastUpdatedDate = DateTime.Now
					});
				}
			}

			// Cập nhật danh sách photos
			if (updatedPhotos.Any())
			{
				existingLocation.Photos = updatedPhotos;
			}
			else
			{
				// CASE 3: Không có cả Photos và files -> xóa hết
				existingLocation.Photos.Clear();
			}

			_unitOfWork.GetRepositoryByEntity<Location>().Update(existingLocation);

			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<LocationResponse>(existingLocation);
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			return await _unitOfWork.LocationRepository.DeleteAsync(id);
		}

		#region Helper method
		private async Task<string> SaveImage(IFormFile file)
		{
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

		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

		private async Task<ApplicationUser> GetAuthenticatedUserAsync()
		{
			var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
			if (user == null)
			{
				throw new UnauthorizedAccessException("User not authenticated");
			}
			return user;
		}
		#endregion
	}
}