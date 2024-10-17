using AutoMapper;
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

namespace PRN231.ExploreNow.Services.Services
{
	public class PostsService : IPostsService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;

		public PostsService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
		}

		public async Task<List<PostsResponse>> GetAllPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm)
		{
			return await _unitOfWork.PostsRepository.GetAllPostsAsync(page, pageSize, postsStatus, searchTerm);
		}

		public async Task<PostsResponse> GetPostsByIdAsync(Guid postsId)
		{
			var post = await _unitOfWork.GetRepository<IPostsRepository>().GetQueryable()
										.Where(p => p.Id == postsId && !p.IsDeleted)
										.Include(p => p.Comments.Where(c => !c.IsDeleted))
										.Include(p => p.Photos.Where(ph => !ph.IsDeleted))
										.Include(p => p.User)
										.FirstOrDefaultAsync();

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
												.FirstOrDefaultAsync();

			// Validate status update
			if (postsRequest.Status.HasValue)
			{
				if ((existingPost.Status == PostsStatus.Approved || existingPost.Status == PostsStatus.Rejected)
					&& postsRequest.Status == PostsStatus.Pending)
				{
					throw new InvalidOperationException("Cannot update status to Pending once it has been Approved or Rejected.");
				}
			}

			existingPost.Content = string.IsNullOrEmpty(postsRequest.Content) ? existingPost.Content : postsRequest.Content;
			existingPost.Status = postsRequest.Status.HasValue ? postsRequest.Status.Value : existingPost.Status;
			existingPost.LastUpdatedBy = user.UserName;
			existingPost.LastUpdatedDate = DateTime.UtcNow;

			// Process and delete all comments if required
			if (postsRequest.RemoveAllComments.HasValue && postsRequest.RemoveAllComments.Value)
			{
				foreach (var comment in existingPost.Comments)
				{
					comment.IsDeleted = true;
					comment.LastUpdatedBy = user.UserName;
					comment.LastUpdatedDate = DateTime.UtcNow;
				}
			}
			// Handle deletion of specific comments if required
			else if (postsRequest.CommentsToRemove != null && postsRequest.CommentsToRemove.Count > 0)
			{
				foreach (var commentId in postsRequest.CommentsToRemove)
				{
					if (Guid.TryParse(commentId, out var commentGuid))
					{
						var comment = existingPost.Comments.FirstOrDefault(c => c.Id == commentGuid);

						if (comment == null || comment.IsDeleted)
						{
							throw new InvalidOperationException($"Comment with ID {commentId} not found or has already been deleted.");
						}

						comment.IsDeleted = true;
						comment.LastUpdatedBy = user.UserName;
						comment.LastUpdatedDate = DateTime.UtcNow;
					}
				}
			}

			// Process and delete all photos if required
			if (postsRequest.RemoveAllPhotos.HasValue && postsRequest.RemoveAllPhotos.Value)
			{
				foreach (var photo in existingPost.Photos)
				{
					photo.IsDeleted = true;
					photo.LastUpdatedBy = user.UserName;
					photo.LastUpdatedDate = DateTime.UtcNow;
				}
			}
			// Handle deletion of specific photos if required
			else if (postsRequest.PhotosToRemove != null && postsRequest.PhotosToRemove.Count > 0)
			{
				foreach (var photoId in postsRequest.PhotosToRemove)
				{
					if (Guid.TryParse(photoId, out var photoGuid))
					{
						var photo = existingPost.Photos.FirstOrDefault(p => p.Id == photoGuid);

						if (photo == null || photo.IsDeleted)
						{
							throw new InvalidOperationException($"Photo with ID {photoId} not found or has already been deleted.");
						}

						photo.IsDeleted = true;
						photo.LastUpdatedBy = user.UserName;
						photo.LastUpdatedDate = DateTime.UtcNow;
					}
				}
			}

			await _unitOfWork.GetRepository<IPostsRepository>().UpdateAsync(existingPost);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<PostsResponse>(existingPost);
		}

		public async Task<bool> DeletePostAsync(Guid postsId)
		{
			return await _unitOfWork.PostsRepository.DeleteAsync(postsId);
		}

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
	}
}
