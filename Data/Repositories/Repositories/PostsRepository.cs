using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class PostsRepository : BaseRepository<Posts>, IPostsRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;

		public PostsRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor,
			UserManager<ApplicationUser> userManager, IMapper mapper) : base(context)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
		}

		public async Task<List<PostsResponse>> GetAllPostsAsync(int page, int pageSize, PostsStatus? postsStatus, string? searchTerm)
		{
			var query = GetQueryable(p => !p.IsDeleted)
					   .Include(p => p.Photos.Where(ph => !ph.IsDeleted))
					   .Include(p => p.Comments.Where(c => !c.IsDeleted))
					   .Include(p => p.User)
					   .AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(p => p.Content.Contains(searchTerm) || p.Rating.ToString().Contains(searchTerm));
			}

			if (postsStatus.HasValue)
			{
				query = query.OrderBy(l => l.Status == postsStatus.Value);
			}

			var posts = await query.Skip((page - 1) * pageSize)
								   .Take(pageSize)
								   .ToListAsync();

			return _mapper.Map<List<PostsResponse>>(posts);
		}

		public async Task<PostsResponse> GetPostsByIdAsync(Guid postsId)
		{
			var user = await GetAuthenticatedUserAsync();

			var post = await GetQueryable(p => p.Id == postsId && !p.IsDeleted)
							.Include(p => p.Comments.Where(c => !c.IsDeleted))
							.Include(p => p.Photos.Where(ph => !ph.IsDeleted))
							.Include(p => p.User)
							.FirstOrDefaultAsync();

			if (post == null)
			{
				throw new InvalidOperationException($"Posts with ID {postsId} not found or has been deleted.");
			}

			return post == null ? null : _mapper.Map<PostsResponse>(post);
		}

		public async Task<PostsResponse> UpdatePostsAsync(Posts posts, PostsRequest postsRequest)
		{
			var user = await GetAuthenticatedUserAsync();

			var existingPost = await GetQueryable(p => p.Id == posts.Id && !p.IsDeleted)
									.Include(p => p.Comments.Where(c => !c.IsDeleted))
									.Include(p => p.Photos.Where(ph => !ph.IsDeleted))
									.Include(p => p.User)
									.FirstOrDefaultAsync();

			if (existingPost == null)
			{
				throw new InvalidOperationException("Existing post not found.");
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
			else if (postsRequest.CommentsToRemove != null && postsRequest.CommentsToRemove.Any())
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

						if (comment != null)
						{
							comment.IsDeleted = true;
							comment.LastUpdatedBy = user.UserName;
							comment.LastUpdatedDate = DateTime.UtcNow;
						}
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
			else if (postsRequest.PhotosToRemove != null && postsRequest.PhotosToRemove.Any())
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

						if (photo != null)
						{
							photo.IsDeleted = true;
							photo.LastUpdatedBy = user.UserName;
							photo.LastUpdatedDate = DateTime.UtcNow;
						}
					}
				}
			}

			Update(existingPost);
			await _context.SaveChangesAsync();

			var response = _mapper.Map<PostsResponse>(existingPost);

			// Filter out deleted comments and photos in the response
			if (postsRequest.RemoveAllComments.HasValue && postsRequest.RemoveAllComments.Value)
			{
				response.Comments = new List<CommentsResponse>();
			}
			else if (postsRequest.CommentsToRemove != null && postsRequest.CommentsToRemove.Any())
			{
				response.Comments = response.Comments
					.Where(c => !postsRequest.CommentsToRemove.Contains(c.CommentsId.ToString()))
					.ToList();
			}

			if (postsRequest.RemoveAllPhotos.HasValue && postsRequest.RemoveAllPhotos.Value)
			{
				response.Photos = new List<PhotoResponse>();
			}
			else if (postsRequest.PhotosToRemove != null && postsRequest.PhotosToRemove.Any())
			{
				response.Photos = response.Photos
					.Where(p => !postsRequest.PhotosToRemove.Contains(p.Id.ToString()))
					.ToList();
			}

			return response;
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
