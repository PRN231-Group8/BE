using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
	public class TourTimeStampRepository : BaseRepository<TourTimestamp>, ITourTimeStampRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly TourRepository _tourRepository;
		private readonly IMapper _mapper;

		public TourTimeStampRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor
			, UserManager<ApplicationUser> userManager, IMapper mapper, TourRepository tourRepository) : base(context)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
			_tourRepository = tourRepository;
		}

		public async Task<List<TourTimeStampResponse>> GetAllTourTimestampsAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm)
		{
			var query = GetQueryable(p => !p.IsDeleted && !p.Tour.IsDeleted)
					   .Include(p => p.Tour)
					   .AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(tt =>
					tt.Title.Contains(searchTerm) ||
					tt.Description.Contains(searchTerm));
			}

			if (sortByTime.HasValue)
			{
				query = query.OrderBy(tt => Math.Abs((tt.PreferredTimeSlot.StartTime - sortByTime.Value).TotalMinutes));
			}
			else
			{
				query = query.OrderBy(tt => tt.PreferredTimeSlot.StartTime);
			}

			var tourTimestamps = await query.Skip((page - 1) * pageSize)
											.Take(pageSize)
											.ToListAsync();

			return _mapper.Map<List<TourTimeStampResponse>>(tourTimestamps);
		}

		public async Task<TourTimeStampResponse> GetByIdAsync(Guid id)
		{
			var tourtimestamps = await GetQueryable(l => l.Id == id && !l.IsDeleted && !l.Tour.IsDeleted)
									  .Include(t => t.Tour)
									  .FirstOrDefaultAsync();

			return _mapper.Map<TourTimeStampResponse>(tourtimestamps);
		}

		public async Task<List<TourTimeStampResponse>> CreateMultipleAsync(List<TourTimestamp> tourTimestamps)
		{
			var user = await GetAuthenticatedUserAsync();
			var today = DateTime.Today;

			foreach (var timestamp in tourTimestamps)
			{
				//Check for time overlap
				if (await IsTimeOverlapping(timestamp.TourId, timestamp.PreferredTimeSlot.StartTime, timestamp.PreferredTimeSlot.EndTime))
				{
					throw new InvalidOperationException($"Time slot from {timestamp.PreferredTimeSlot.StartTime} to {timestamp.PreferredTimeSlot.EndTime} overlaps with an existing timestamp.");
				}

				timestamp.Code = GenerateUniqueCode();
				timestamp.CreatedBy = user.UserName;
				timestamp.CreatedDate = DateTime.Now;
				timestamp.LastUpdatedDate = DateTime.Now;
				timestamp.LastUpdatedBy = user.UserName;
			}

			AddRange(tourTimestamps);
			await _context.SaveChangesAsync();

			return _mapper.Map<List<TourTimeStampResponse>>(tourTimestamps);
		}

		public async Task<TourTimeStampResponse> UpdateAsync(TourTimestamp tourTimestamp)
		{
			var user = await GetAuthenticatedUserAsync();

			var existingTourTimestamp = await GetQueryable(tt => tt.Id == tourTimestamp.Id && !tt.IsDeleted && !tt.Tour.IsDeleted)
											 .Include(tt => tt.Tour)
											 .FirstOrDefaultAsync();

			if (existingTourTimestamp == null)
			{
				throw new InvalidOperationException("Existing tour timestamp not found.");
			}

			existingTourTimestamp.Description = tourTimestamp.Description;
			existingTourTimestamp.Title = tourTimestamp.Title;
			existingTourTimestamp.PreferredTimeSlot = tourTimestamp.PreferredTimeSlot;
			existingTourTimestamp.LastUpdatedBy = user.UserName;
			existingTourTimestamp.LastUpdatedDate = DateTime.Now;

			Update(existingTourTimestamp);
			await _context.SaveChangesAsync();

			return _mapper.Map<TourTimeStampResponse>(existingTourTimestamp);
		}

		// Generate random Code
		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

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

		// Check for time overlap
		private async Task<bool> IsTimeOverlapping(Guid tourId, TimeSpan startTime, TimeSpan endTime)
		{
			var existingTimestamps = await GetQueryable(t => t.TourId == tourId && !t.IsDeleted).ToListAsync();

			foreach (var existing in existingTimestamps)
			{
				if (existing.PreferredTimeSlot.StartTime < endTime &&
					existing.PreferredTimeSlot.EndTime > startTime)
				{
					return true; // Overlapping found
				}
			}

			return false; // No overlap
		}

		private bool IsTimeOverlapping(TimeSlot slot1, TimeSlot slot2)
		{
			return slot1.StartTime < slot2.EndTime && slot2.StartTime < slot1.EndTime;
		}

		private async Task<List<TourTimestamp>> GetExistingTimestampsForTour(Guid tourId)
		{
			return await GetQueryable(t => t.TourId == tourId && !t.IsDeleted)
				.ToListAsync();
		}
	}
}
