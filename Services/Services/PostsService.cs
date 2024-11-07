using AutoMapper;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace PRN231.ExploreNow.Services.Services
{
	public class PostsService : IPostsService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;
		private Cloudinary _cloudinary;
		private readonly IConfiguration _configuration;
		private const int MAX_IMAGES = 5;
		private const int MAX_FILE_SIZE = 10 * 1024 * 1024;
		private const string SIZE_LIMIT_TEXT = "10MB";

		public PostsService(
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor,
			UserManager<ApplicationUser> userManager,
			IMapper mapper,
			IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
			_configuration = configuration;

			var account = new Account(
				_configuration["CloudinarySetting:CloudName"],
				_configuration["CloudinarySetting:ApiKey"],
				_configuration["CloudinarySetting:ApiSecret"]
			);
			_cloudinary = new Cloudinary(account);
		}

		public async Task<List<PostsResponse>> GetAllPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm)
		{
			return await _unitOfWork.GetRepository<IPostsRepository>().GetAllPostsAsync(page, pageSize, postsStatus, searchTerm);
		}

		public async Task<List<PostsResponse>> GetAllPendingPostsAsync(int page, int pageSize, string? searchTerm)
		{
			return await _unitOfWork.GetRepository<IPostsRepository>().GetAllPendingPostsAsync(page, pageSize, searchTerm);
		}

		public async Task<List<PostsResponse>> GetUserPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm)
		{
			var user = await GetAuthenticatedUserAsync();
			return await _unitOfWork.GetRepository<IPostsRepository>().GetUserPostsAsync(user.Id, page, pageSize, postsStatus, searchTerm);
		}

		public async Task<PostsResponse> GetPostsByIdAsync(Guid postsId)
		{
			var user = await GetAuthenticatedUserAsync();

			var post = await _unitOfWork.GetRepository<IPostsRepository>().GetQueryable()
										.AsNoTracking()
										.Where(p => p.Id == postsId && !p.IsDeleted)
										.Include(p => p.Comments.Where(c => !c.IsDeleted))
										.Include(p => p.Photos.Where(ph => !ph.IsDeleted))
										.Include(p => p.User)
										.SingleOrDefaultAsync();

			var isModerator = await _userManager.IsInRoleAsync(user, "MODERATOR");
			var isOwner = post.UserId == user.Id;
			if (!isModerator && !isOwner)
			{
				throw new UnauthorizedAccessException("You don't have permission to view this post. Only the post owner or moderators can view post details.");
			}

			return _mapper.Map<PostsResponse>(post);
		}

		public async Task<PostsResponse> UpdatePostsAsync(Guid postsId, PostsRequest postsRequest)
		{
			var user = await GetAuthenticatedUserAsync();

			var existingPost = await _unitOfWork.GetRepository<IPostsRepository>().GetQueryable()
												.Where(p => p.Id == postsId && !p.IsDeleted)
												.Include(p => p.Comments.Where(c => !c.IsDeleted))
												.Include(p => p.Photos.Where(ph => !ph.IsDeleted))
												.Include(p => p.User)
												.SingleAsync();
			// Check user roles and permissions
			var isModerator = await _userManager.IsInRoleAsync(user, "MODERATOR");
			var isOwner = existingPost.UserId == user.Id;
			if (!isModerator && !isOwner)
			{
				throw new UnauthorizedAccessException("You don't have permission to delete this post. Only the post owner or moderators can delete posts.");
			}
			// Validate status update
			if (postsRequest.Status.HasValue)
			{
				if ((existingPost.Status == PostsStatus.Approved || existingPost.Status == PostsStatus.Rejected)
					&& postsRequest.Status == PostsStatus.Pending)
				{
					throw new InvalidOperationException("Cannot update status to Pending once it has been Approved or Rejected.");
				}
			}

			// Get repositories
			var commentsRepo = _unitOfWork.GetRepositoryByEntity<Comments>();
			var photosRepo = _unitOfWork.GetRepositoryByEntity<Photo>();

			var currentUser = user.UserName;
			var now = DateTime.UtcNow;

			existingPost.Content = postsRequest.Content;
			existingPost.Status = postsRequest.Status.Value;
			existingPost.LastUpdatedBy = user.UserName;
			existingPost.LastUpdatedDate = DateTime.UtcNow;

			if (postsRequest.RemoveAllComments.HasValue && postsRequest.RemoveAllComments.Value)
			{
				var commentsToDelete = existingPost.Comments.Where(c => !c.IsDeleted);
				foreach (var comment in commentsToDelete)
				{
					comment.LastUpdatedBy = currentUser;
					comment.LastUpdatedDate = now;
					await commentsRepo.DeleteAsync(comment.Id);
				}
			}
			else if (postsRequest.CommentsToRemove != null && postsRequest.CommentsToRemove.Count > 0)
			{
				foreach (var commentId in postsRequest.CommentsToRemove)
				{
					if (Guid.TryParse(commentId, out var commentGuid))
					{
						var comment = existingPost.Comments.FirstOrDefault(c => c.Id == commentGuid);
						if (comment == null)
						{
							throw new InvalidOperationException($"Comment with ID {commentId} not found or has already been deleted.");
						}

						comment.LastUpdatedBy = currentUser;
						comment.LastUpdatedDate = now;
						await commentsRepo.DeleteAsync(comment.Id);
					}
				}
			}

			if (postsRequest.RemoveAllPhotos.HasValue && postsRequest.RemoveAllPhotos.Value)
			{
				var photosToDelete = existingPost.Photos.Where(p => !p.IsDeleted);
				foreach (var photo in photosToDelete)
				{
					photo.LastUpdatedBy = currentUser;
					photo.LastUpdatedDate = now;
					await photosRepo.DeleteAsync(photo.Id);
				}
			}
			else if (postsRequest.PhotosToRemove != null && postsRequest.PhotosToRemove.Count > 0)
			{
				foreach (var photoId in postsRequest.PhotosToRemove)
				{
					if (Guid.TryParse(photoId, out var photoGuid))
					{
						var photo = existingPost.Photos.FirstOrDefault(p => p.Id == photoGuid);
						if (photo == null)
						{
							throw new InvalidOperationException($"Photo with ID {photoId} not found or has already been deleted.");
						}

						photo.LastUpdatedBy = currentUser;
						photo.LastUpdatedDate = now;
						await photosRepo.DeleteAsync(photo.Id);
					}
				}
			}

			await _unitOfWork.GetRepository<IPostsRepository>().UpdateAsync(existingPost);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<PostsResponse>(existingPost);
		}

		public async Task<bool> DeletePostAsync(Guid postsId)
		{
			var user = await GetAuthenticatedUserAsync();

			var post = await _unitOfWork.GetRepository<IPostsRepository>()
										.GetQueryable()
										.Include(p => p.Comments)
										.Include(p => p.Photos)
										.Include(p => p.User)
										.SingleOrDefaultAsync(p => p.Id == postsId);

			// Check user roles and permissions
			var isModerator = await _userManager.IsInRoleAsync(user, "MODERATOR");
			var isOwner = post.UserId == user.Id;
			if (!isModerator && !isOwner)
			{
				throw new UnauthorizedAccessException("You don't have permission to delete this post. Only the post owner or moderators can delete posts.");
			}

			var currentUser = user.UserName;
			var now = DateTime.UtcNow;

			post.LastUpdatedBy = currentUser;
			post.LastUpdatedDate = now;

			await _unitOfWork.SaveChangesAsync();
			return await _unitOfWork.GetRepository<IPostsRepository>().DeleteAsync(postsId);
		}

		public async Task<PostsResponse> CreatePost(CreatePostRequest createPostRequest)
		{
			var user = await GetAuthenticatedUserAsync();

			if (user == null || string.IsNullOrEmpty(user.UserName))
			{
				throw new InvalidOperationException("User is not authenticated.");
			}

			if (createPostRequest.Photos == null || !createPostRequest.Photos.Any())
			{
				throw new ArgumentException("At least one photo is required.");
			}
			if (createPostRequest.Photos.Count > MAX_IMAGES)
			{
				throw new ArgumentException("You can upload up to {MAX_IMAGES} images only.");
			}

			foreach (var file in createPostRequest.Photos)
			{
				if (file.Length > MAX_FILE_SIZE)
				{
					throw new ArgumentException($"File {file.FileName} exceeds the {SIZE_LIMIT_TEXT} size limit.");
				}
			}

			// Prepare photos
			var photos = new List<Photo>();
			foreach (var file in createPostRequest.Photos)
			{
				var altText = Path.GetFileNameWithoutExtension(file.FileName);

				using var stream = file.OpenReadStream();
				var uploadParams = new ImageUploadParams
				{
					File = new FileDescription(file.FileName, stream),
				};

				var uploadResult = await _cloudinary.UploadAsync(uploadParams);

				photos.Add(new Photo
				{
					Url = uploadResult.SecureUrl.ToString(),
					Alt = altText,
					Code = GenerateUniqueCode(),
					CreatedBy = user.UserName
				});
			}

			// Create the new post entity
			var post = new Posts
			{
				Content = createPostRequest.Content,
				Status = PostsStatus.Pending,
				UserId = user.Id,
				CreatedBy = user.UserName,
				CreatedDate = DateTime.UtcNow,
				Photos = photos,
				LastUpdatedBy = user.UserName,
				Code = GenerateUniqueCode()
			};

			await _unitOfWork.GetRepository<IPostsRepository>().AddAsync(post);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<PostsResponse>(post);
		}

		#region Helper method
		// Check the user is authenticated
		private async Task<ApplicationUser> GetAuthenticatedUserAsync()
		{
			var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
			if (user == null)
			{
				throw new UnauthorizedAccessException("User not authenticated");
			}
			return user;
		}

		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
		#endregion
	}
}