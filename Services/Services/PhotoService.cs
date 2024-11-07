using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PRN231.ExploreNow.Services.Services
{
	public class PhotoService : IPhotoService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly Cloudinary _cloudinary;

		public PhotoService(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;

			var account = new Account(
				configuration["CloudinarySetting:CloudName"],
				configuration["CloudinarySetting:ApiKey"],
				configuration["CloudinarySetting:ApiSecret"]
			);
			_cloudinary = new Cloudinary(account);
		}

		public async Task<PhotoResponse> GetPhotoByIdAsync(Guid id)
		{
			var photo = await _unitOfWork.GetRepository<IPhotoRepository>()
								  .GetQueryable()
								  .Where(p => p.Id == id && !p.IsDeleted)
								  .SingleOrDefaultAsync();

			if (photo == null)
			{
				throw new KeyNotFoundException($"Photo with ID {id} not found.");
			}

			return new PhotoResponse
			{
				Id = photo.Id,
				Url = photo.Url,
				Alt = photo.Alt,
				PostId = photo.PostId
			};
		}

		public async Task<PhotoResponse> UpdatePhotoAsync(Guid photoId, Guid postId, IFormFile file)
		{
			// Fetch the photo by ID and ensure it belongs to the correct post
			var photoRepo = _unitOfWork.GetRepository<IPhotoRepository>();
			var photo = await photoRepo.GetQueryable()
										.SingleOrDefaultAsync(p => p.Id == photoId && p.PostId == postId);

			if (photo == null)
			{
				throw new KeyNotFoundException($"Photo with ID {photoId} for Post ID {postId} not found.");
			}

			// Upload the new image to Cloudinary
			using var stream = file.OpenReadStream();
			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(file.FileName, stream)
			};
			var uploadResult = await _cloudinary.UploadAsync(uploadParams);

			// Update the photo URL with the new URL
			photo.Url = uploadResult.SecureUrl.ToString();
			photo.LastUpdatedDate = DateTime.UtcNow;

			// Save changes
			await photoRepo.UpdateAsync(photo);
			await _unitOfWork.SaveChangesAsync();

			// Return the updated photo details
			return new PhotoResponse
			{
				Id = photo.Id,
				Url = photo.Url,
				Alt = photo.Alt,
				PostId = photo.PostId
			};
		}
	}
}
