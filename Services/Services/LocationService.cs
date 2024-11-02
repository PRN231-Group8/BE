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


        public async Task<List<LocationResponse>> GetAllLocationsAsync(int page, int pageSize,
            WeatherStatus? sortByStatus, string? searchTerm)
        {
            return await _unitOfWork.LocationRepository.GetAllLocationsAsync(page, pageSize, sortByStatus, searchTerm);
        }

        public async Task<LocationResponse> GetByIdAsync(Guid id)
        {
            var location = await _unitOfWork.GetRepositoryByEntity<Location>().GetQueryable()
                .Where(l => l.Id == id && !l.IsDeleted)
                .Include(l => l.Photos)
                .FirstOrDefaultAsync();
            return _mapper.Map<LocationResponse>(location);
        }

        public async Task<LocationResponse> CreateAsync(LocationCreateRequest locationsRequest, List<IFormFile> files)
        {
            var location = _mapper.Map<Location>(locationsRequest);

            var currentUser = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
            var currentUserName = currentUser?.UserName ?? "Admin";
            location.CreatedBy = currentUserName;
            location.CreatedDate = DateTime.Now;
            location.LastUpdatedBy = currentUserName;
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


        public async Task<LocationResponse> UpdateAsync(Guid id, LocationsRequest locationsRequest,
            List<IFormFile> files = null)
        {
            var currentUser = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
            var currentUserName = currentUser?.UserName ?? "Admin";

            var existingLocation = await _unitOfWork.GetRepositoryByEntity<Location>()
                .GetQueryable()
                .Where(l => l.Id == id && !l.IsDeleted)
                .Include(l => l.Photos)
                .FirstOrDefaultAsync();

            if (existingLocation == null)
            {
                throw new Exception("Location not found.");
            }

            // Update basic properties from LocationsRequest
            _mapper.Map(locationsRequest, existingLocation);
            existingLocation.LastUpdatedBy = currentUserName;
            existingLocation.LastUpdatedDate = DateTime.Now;

            if (locationsRequest.Photos != null && locationsRequest.Photos.Any())
            {
                // Keep or update photos with IDs in `Photos` list
                foreach (var photoRequest in locationsRequest.Photos)
                {
                    if (photoRequest.Id != null)
                    {
                        var existingPhoto = existingLocation.Photos.FirstOrDefault(p => p.Id == photoRequest.Id.Value);
                        if (existingPhoto != null && existingPhoto.Url != photoRequest.Url)
                        {
                            existingPhoto.Url = photoRequest.Url;
                            existingPhoto.LastUpdatedBy = currentUserName;
                            existingPhoto.LastUpdatedDate = DateTime.Now;
                        }
                    }
                }

                // If `Files` is provided, add new photos from files without clearing existing `Photos`
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var photoUrl = await SaveImage(file);
                        existingLocation.Photos.Add(new Photo
                        {
                            Url = photoUrl,
                            Alt = file.FileName,
                            Code = GenerateUniqueCode(),
                            CreatedBy = currentUserName,
                            CreatedDate = DateTime.Now,
                            LastUpdatedBy = currentUserName,
                            LastUpdatedDate = DateTime.Now
                        });
                    }
                }
                else
                {
                    // No `Files` provided: remove any photos not listed in `Photos`
                    existingLocation.Photos = existingLocation.Photos
                        .Where(p => locationsRequest.Photos.Any(pr => pr.Id == p.Id))
                        .ToList();
                }
            }
            else if (files != null && files.Any())
            {
                // `Photos` is empty, `Files` has entries: Clear existing photos and add new ones
                existingLocation.Photos.Clear();
                foreach (var file in files)
                {
                    var photoUrl = await SaveImage(file);
                    existingLocation.Photos.Add(new Photo
                    {
                        Url = photoUrl,
                        Alt = file.FileName,
                        Code = GenerateUniqueCode(),
                        CreatedBy = currentUserName,
                        CreatedDate = DateTime.Now,
                        LastUpdatedBy = currentUserName,
                        LastUpdatedDate = DateTime.Now
                    });
                }
            }
            else
            {
                // Both `Photos` and `Files` are empty: Clear all photos
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
    }
}