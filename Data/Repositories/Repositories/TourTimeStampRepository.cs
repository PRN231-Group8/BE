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
	public class TourTimeStampRepository : BaseRepository<TourTimestamp>, ITourTimeStampRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;

		public TourTimeStampRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor
			 , UserManager<ApplicationUser> userManager, IMapper mapper) : base(context)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
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

			var tourTimestamps = await query.Skip((page - 1) * pageSize)
											.Take(pageSize)
											.ToListAsync();

			// If sortByTime is provided, sort by the absolute difference between start time and sortByTime
			// If the request you send is 12:00:00, the function will return the times before and after and the closest distance, the function will take that value.
			if (sortByTime.HasValue)
			{
				// Sort the tour list based on the difference between each tour's start time and sortByTime
				tourTimestamps = tourTimestamps
								 .OrderBy(tt => Math.Abs((tt.PreferredTimeSlot.StartTime - sortByTime.Value).TotalMinutes))
								 .ToList();
			}
			else
			{
				// Without sortByTime, sort by starting time in ascending order
				tourTimestamps = tourTimestamps
								 .OrderBy(tt => tt.PreferredTimeSlot.StartTime)
								 .ToList();
			}

			return _mapper.Map<List<TourTimeStampResponse>>(tourTimestamps);
		}

		public async Task<TourTimeStampResponse> GetByIdAsync(Guid id)
		{
			var tourtimestamps = await GetQueryable(l => l.Id == id && !l.IsDeleted && !l.Tour.IsDeleted)
									  .Include(t => t.Tour)
									  .FirstOrDefaultAsync();

			if (tourtimestamps == null)
			{
				throw new InvalidOperationException($"TourTimestamp with ID {id} not found or has been deleted.");
			}

			return _mapper.Map<TourTimeStampResponse>(tourtimestamps);
		}

		public async Task<List<TourTimeStampResponse>> CreateMultipleAsync(List<TourTimestamp> tourTimestamps)
		{
			var user = await GetAuthenticatedUserAsync();

			if (tourTimestamps == null || tourTimestamps.Count == 0)
			{
				throw new ArgumentException("The list of tour timestamps is empty or null.");
			}

			// Group timestamps by TourId
			var groupedTimestamps = tourTimestamps.GroupBy(t => t.TourId);

			foreach (var group in groupedTimestamps)
			{
				var tourId = group.Key;

				// Validate TourId
				if (!await TourExistsAsync(tourId))
				{
					throw new InvalidOperationException($"Tour with ID {tourId} does not exist.");
				}

				var timestampsForTour = group.ToList();

				// Get existing timestamps for this tour
				var existingTimestamps = await GetQueryable(t => t.TourId == tourId && !t.IsDeleted).ToListAsync();

				// Check for overlaps within new timestamps and with existing timestamps
				var allTimestamps = existingTimestamps.Concat(timestampsForTour).ToList();

				var overlaps = FindOverlaps(allTimestamps);
				if (overlaps.Count > 0)
				{
					var overlapDescriptions = overlaps.Select(o =>
						$"Overlap detected: {o.Item1.PreferredTimeSlot.StartTime} - {o.Item1.PreferredTimeSlot.EndTime} conflicts with {o.Item2.PreferredTimeSlot.StartTime} - {o.Item2.PreferredTimeSlot.EndTime}");
					throw new InvalidOperationException($"Time slot overlaps detected: {string.Join(", ", overlapDescriptions)}");
				}

				foreach (var timestamp in timestampsForTour)
				{
					timestamp.Code = GenerateUniqueCode();
					timestamp.CreatedBy = user.UserName;
					timestamp.CreatedDate = DateTime.Now;
					timestamp.LastUpdatedDate = DateTime.Now;
					timestamp.LastUpdatedBy = user.UserName;
				}
			}

			AddRange(tourTimestamps);
			await _context.SaveChangesAsync();

			return _mapper.Map<List<TourTimeStampResponse>>(tourTimestamps);
		}

		public async Task<TourTimeStampResponse> UpdateAsync(TourTimestamp tourTimestamp, TourTimeStampRequest tourTimeStampRequest)
		{
			var user = await GetAuthenticatedUserAsync();

			var existingTourTimestamp = await GetQueryable(tt => tt.Id == tourTimestamp.Id && !tt.IsDeleted && !tt.Tour.IsDeleted)
											 .Include(tt => tt.Tour)
											 .FirstOrDefaultAsync();

			if (existingTourTimestamp == null)
			{
				throw new InvalidOperationException("Existing tour timestamp not found.");
			}

			// Validate TourId
			if (!await TourExistsAsync(tourTimestamp.TourId))
			{
				throw new InvalidOperationException($"Tour with ID {tourTimestamp.TourId} does not exist.");
			}

			// Get all other timestamps for the same tour
			var allTimestamps = await GetQueryable(tt => tt.TourId == existingTourTimestamp.TourId && tt.Id != existingTourTimestamp.Id && !tt.IsDeleted)
									 .ToListAsync();

			// Add the updated timestamp to the list for overlap check
			allTimestamps.Add(tourTimestamp);

			// Check for overlaps
			var overlaps = FindOverlaps(allTimestamps);
			if (overlaps.Count > 0)
			{
				var overlapDescriptions = overlaps.Select(o =>
					$"Overlap detected: {o.Item1.PreferredTimeSlot.StartTime} - {o.Item1.PreferredTimeSlot.EndTime} conflicts with {o.Item2.PreferredTimeSlot.StartTime} - {o.Item2.PreferredTimeSlot.EndTime}");
				throw new InvalidOperationException($"Time slot overlaps detected: {string.Join(", ", overlapDescriptions)}");
			}

			existingTourTimestamp.Title = string.IsNullOrWhiteSpace(tourTimeStampRequest.Title) ? existingTourTimestamp.Title : tourTimeStampRequest.Title;
			existingTourTimestamp.Description = string.IsNullOrWhiteSpace(tourTimeStampRequest.Description) ? existingTourTimestamp.Description : tourTimeStampRequest.Description;
			existingTourTimestamp.PreferredTimeSlot = tourTimestamp.PreferredTimeSlot;
			existingTourTimestamp.LastUpdatedBy = user.UserName;
			existingTourTimestamp.LastUpdatedDate = DateTime.Now;

			await UpdateAsync(existingTourTimestamp);
			await _context.SaveChangesAsync();

			return _mapper.Map<TourTimeStampResponse>(existingTourTimestamp);
		}

		// Generate random Code
		private static string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

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

		// Check if a tour exists
		private async Task<bool> TourExistsAsync(Guid tourId)
		{
			return await _context.Tours.AnyAsync(t => t.Id == tourId && !t.IsDeleted);
		}

		// Check for time overlap
		private static List<(TourTimestamp, TourTimestamp)> FindOverlaps(List<TourTimestamp> timestamps)
		{
			var overlaps = new List<(TourTimestamp, TourTimestamp)>();
			for (int i = 0; i < timestamps.Count; i++)
			{
				for (int j = i + 1; j < timestamps.Count; j++)
				{
					if (IsTimeOverlapping(timestamps[i].PreferredTimeSlot, timestamps[j].PreferredTimeSlot))
					{
						overlaps.Add((timestamps[i], timestamps[j]));
					}
				}
			}
			return overlaps;
		}

		private static bool IsTimeOverlapping(TimeSlot slot1, TimeSlot slot2)
		{
			return slot1.StartTime < slot2.EndTime && slot2.StartTime < slot1.EndTime;
		}
	}
}
