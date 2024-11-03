using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories;

namespace PRN231.ExploreNow.BusinessObject.Contracts.Repositories
{
	public class CommentRepository : BaseRepository<Comments>, ICommentRepository
	{
		public CommentRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}
