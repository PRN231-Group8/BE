using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class PostsRepository : BaseRepository<Posts>, IPostsRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public PostsRepository(ApplicationDbContext context, IMapper mapper) : base(context)
		{
			_context = context;
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
	}
}
