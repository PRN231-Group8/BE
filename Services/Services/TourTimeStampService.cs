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
	public class TourTimeStampService : ITourTimeStampService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;

		public TourTimeStampService(IUnitOfWork unitOfWork, IMapper mapper,
			IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
		}

		public async Task<List<TourTimeStampResponse>> GetAllTourTimeStampAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm)
		{
			return await _unitOfWork.GetRepository<ITourTimeStampRepository>().GetAllTourTimestampsAsync(page, pageSize, sortByTime, searchTerm);
		}

		public async Task<TourTimeStampResponse> GetTourTimeStampByIdAsync(Guid tourTimeStampId)
		{
			var tourTimestamp = await _unitOfWork.GetRepository<ITourTimeStampRepository>().GetQueryable()
							   .Where(l => l.Id == tourTimeStampId && !l.IsDeleted && !l.Tour.IsDeleted)
							   .Include(t => t.Tour)
							   .Include(tt => tt.Location)
							   .ThenInclude(l => l.Photos)
							   .FirstOrDefaultAsync();

			return _mapper.Map<TourTimeStampResponse>(tourTimestamp);
		}

		public async Task<List<TourTimeStampResponse>> CreateBatchTourTimeStampsAsync(List<TourTimeStampRequest> tourTimeStampRequests)
		{
			var user = await GetAuthenticatedUserAsync();

			var tourTimestamps = _mapper.Map<List<TourTimestamp>>(tourTimeStampRequests);

			// Group timestamps by TourId and LocationId
			var groupedTimestamps = tourTimestamps.GroupBy(t => new { t.TourId, t.LocationId });

			foreach (var group in groupedTimestamps)
			{
				var tourId = group.Key.TourId;
				var locationId = group.Key.LocationId;

				var timestampsForTourAndLocation = group.ToList();

				// Get existing timestamps for this tour
				var existingTimestamps = await _unitOfWork.GetRepository<ITourTimeStampRepository>().GetQueryable()
										.Where(t => t.TourId == tourId && t.LocationId == locationId && !t.IsDeleted)
										.ToListAsync();

				// Check for overlaps within new timestamps and with existing timestamps
				var allTimestamps = existingTimestamps.Concat(timestampsForTourAndLocation).ToList();

				var overlaps = FindOverlaps(allTimestamps);
				if (overlaps.Count > 0)
				{
					var overlapDescriptions = overlaps.Select(o =>
						$"Overlap detected: {o.Item1.PreferredTimeSlot.StartTime} - {o.Item1.PreferredTimeSlot.EndTime} conflicts with {o.Item2.PreferredTimeSlot.StartTime} - {o.Item2.PreferredTimeSlot.EndTime}");
					throw new InvalidOperationException($"Time slot overlaps detected: {string.Join(", ", overlapDescriptions)}");
				}

				foreach (var timestamp in timestampsForTourAndLocation)
				{
					timestamp.Code = GenerateUniqueCode();
					timestamp.CreatedBy = user.UserName;
					timestamp.CreatedDate = DateTime.Now;
					timestamp.LastUpdatedDate = DateTime.Now;
					timestamp.LastUpdatedBy = user.UserName;
				}
			}

			await _unitOfWork.GetRepository<ITourTimeStampRepository>().AddRangeAsync(tourTimestamps);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<List<TourTimeStampResponse>>(tourTimestamps);
		}

		public async Task<TourTimeStampResponse> UpdateTourTimeStampAsync(Guid tourTimeStampId, TourTimeStampRequest tourTimeStampRequest)
		{
			var user = await GetAuthenticatedUserAsync();

			var existingTourTimestamp = await _unitOfWork.GetRepository<ITourTimeStampRepository>().GetQueryable()
									   .Where(tt => tt.Id == tourTimeStampId && !tt.IsDeleted && !tt.Tour.IsDeleted && !tt.Location.IsDeleted)
									   .Include(tt => tt.Tour)
									   .Include(tt => tt.Location)
									   .ThenInclude(l => l.Photos)
									   .FirstOrDefaultAsync();

			// Get all other timestamps for the same tour and location
			var allTimestamps = await _unitOfWork.GetRepository<ITourTimeStampRepository>().GetQueryable()
							   .Where(tt => tt.TourId == existingTourTimestamp.TourId &&
											tt.LocationId == tourTimeStampRequest.LocationId &&
											tt.Id != existingTourTimestamp.Id &&
											!tt.IsDeleted)
							   .ToListAsync();

			var updatedTimestamp = _mapper.Map<TourTimestamp>(tourTimeStampRequest);
			updatedTimestamp.Id = tourTimeStampId;

			// Add the updated timestamp to the list for overlap check
			allTimestamps.Add(updatedTimestamp);

			// Check for overlaps
			var overlaps = FindOverlaps(allTimestamps);
			if (overlaps.Count > 0)
			{
				var overlapDescriptions = overlaps.Select(o =>
					$"Overlap detected: {o.Item1.PreferredTimeSlot.StartTime} - {o.Item1.PreferredTimeSlot.EndTime} conflicts with {o.Item2.PreferredTimeSlot.StartTime} - {o.Item2.PreferredTimeSlot.EndTime}");
				throw new InvalidOperationException($"Time slot overlaps detected: {string.Join(", ", overlapDescriptions)}");
			}

			existingTourTimestamp.Title = tourTimeStampRequest.Title;
			existingTourTimestamp.Description = tourTimeStampRequest.Description;
			existingTourTimestamp.PreferredTimeSlot = updatedTimestamp.PreferredTimeSlot;
			existingTourTimestamp.LocationId = tourTimeStampRequest.LocationId;
			existingTourTimestamp.TourId = tourTimeStampRequest.TourId;
			existingTourTimestamp.LastUpdatedBy = user.UserName;
			existingTourTimestamp.LastUpdatedDate = DateTime.Now;

			await _unitOfWork.GetRepository<ITourTimeStampRepository>().UpdateAsync(existingTourTimestamp);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<TourTimeStampResponse>(existingTourTimestamp);
		}

		public async Task<bool> DeleteAsync(Guid tourTimeStampId)
		{
			return await _unitOfWork.GetRepository<ITourTimeStampRepository>().DeleteAsync(tourTimeStampId);
		}

		// Generate random Code
		private static string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

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
		private static List<(TourTimestamp, TourTimestamp)> FindOverlaps(List<TourTimestamp> timestamps)
		{
			return timestamps
				.SelectMany((t1, i) => timestamps.Skip(i + 1), (t1, t2) => (t1, t2))
				.Where(t => IsTimeOverlapping(t.t1.PreferredTimeSlot, t.t2.PreferredTimeSlot))
				.ToList();
		}

		private static bool IsTimeOverlapping(TimeSlot slot1, TimeSlot slot2)
		{
			return slot1.StartTime < slot2.EndTime && slot2.StartTime < slot1.EndTime;
		}
	}
}
