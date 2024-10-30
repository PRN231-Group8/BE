using Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class MoodRepository : BaseRepository<Moods>, IMoodRepository
	{
		private readonly ApplicationDbContext _context;
		public MoodRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_context = dbContext;
		}

		public async Task<List<Moods>> GetAllAsync(int page, int pageSize, List<string>? searchTerms)
		{
			var query = GetQueryable()
						.Where(m => !m.IsDeleted)
						.AsEnumerable();  // Chuyển sang xử lý phía client

			if (searchTerms != null && searchTerms.Any())
			{
				// Thực hiện lọc sau khi đã chuyển sang client
				query = query.Where(m => searchTerms.Any(term => m.MoodTag.Contains(term)));
			}

			var moods = query.Skip((page - 1) * pageSize)
							 .Take(pageSize)
							 .ToList();  // Sử dụng xử lý đồng bộ hoặc async tùy theo nhu cầu

			return moods;
		}
	}
}
