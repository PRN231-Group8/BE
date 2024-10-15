using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
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
			return await _unitOfWork.PostsRepository.GetPostsByIdAsync(postsId);
		}

		public async Task<PostsResponse> UpdatePostsAsync(Guid postsId, PostsRequest postsRequest)
		{
			var existingPost = _mapper.Map<Posts>(postsRequest);
			existingPost.Id = postsId;
			return await _unitOfWork.PostsRepository.UpdatePostsAsync(existingPost, postsRequest);
		}

		public async Task<bool> DeletePostAsync(Guid postsId)
		{
			var existingPost = await _unitOfWork.PostsRepository.GetPostsByIdAsync(postsId);
			if (existingPost == null)
			{
				throw new InvalidOperationException("Existing posts not found or has already been deleted.");
			}

			return await _unitOfWork.PostsRepository.DeleteAsync(postsId);
		}
	}
}
