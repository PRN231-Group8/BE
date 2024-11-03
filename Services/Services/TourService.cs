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
	public class TourService : ITourService
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _iUnitOfWork;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;

		public TourService(IUnitOfWork iUnitOfWork, IHttpContextAccessor iContextAccessor, IMapper mapper, UserManager<ApplicationUser> userManager)
		{
			_iUnitOfWork = iUnitOfWork;
			_contextAccessor = iContextAccessor;
			_mapper = mapper;
			_userManager = userManager;
		}

		public async Task<(List<TourResponse> Items, int TotalCount)> GetAllToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm)
		{
			return await _iUnitOfWork.GetRepository<ITourRepository>().GetToursAsync(page, pageSize, sortByStatus, searchTerm);
		}

		public async Task<TourResponse> GetById(Guid id)
		{
			var tour = await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetQueryable()
					  .AsSplitQuery()
					  .Where(t => t.Id == id && !t.IsDeleted)
					  .Include(t => t.TourMoods.Where(tm => !tm.IsDeleted && !tm.Mood.IsDeleted))
						 .ThenInclude(tm => tm.Mood)
					  .Include(t => t.LocationInTours.Where(lit => !lit.IsDeleted && !lit.Location.IsDeleted))
						 .ThenInclude(lit => lit.Location)
							.ThenInclude(l => l.Photos.Where(p => !p.IsDeleted))
					  .Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
					  .Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
					  .SingleOrDefaultAsync();

			return _mapper.Map<TourResponse>(tour);
		}

		public async Task<TourResponse> Add(TourRequestModel request)
		{
			await ValidateTourRequest(request);
			var user = await GetAuthenticatedUserAsync();

			var tour = await CreateTourFromRequest(request, user);
			tour.TotalPrice = 0;

			await _iUnitOfWork.GetRepositoryByEntity<Tour>().AddAsync(tour);
			await _iUnitOfWork.SaveChangesAsync();

			await UpdateTourPrice(tour.Id);
			return await GetById(tour.Id);
		}

		public async Task<TourResponse> UpdateAsync(TourRequestModel request, Guid id)
		{
			await ValidateTourRequest(request);
			var user = await GetAuthenticatedUserAsync();

			var existingTour = await GetTourById(id);
			if (existingTour == null)
				throw new KeyNotFoundException($"Tour with ID {id} not found");

			await UpdateTourFromRequest(existingTour, request, user);
			await _iUnitOfWork.SaveChangesAsync();

			await UpdateTourPrice(existingTour.Id);
			return await GetById(existingTour.Id);
		}

		public async Task Delete(Guid id)
		{
			await _iUnitOfWork.GetRepositoryByEntity<Tour>().DeleteAsync(id);
			await _iUnitOfWork.SaveChangesAsync();
		}

		public async Task UpdateTourPrice(Guid tourId)
		{
			var tour = await _iUnitOfWork.GetRepositoryByEntity<Tour>()
				.GetQueryable()
				.Include(t => t.TourTrips.Where(tt => !tt.IsDeleted && tt.TripStatus != TripStatus.CANCELLED))
				.Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
				.SingleOrDefaultAsync(t => t.Id == tourId && !t.IsDeleted);

			if (tour == null)
				return;

			// Get active records
			var activeTourTrips = tour.TourTrips.ToList();
			var activeTransportations = tour.Transportations?.Where(t => !t.IsDeleted).ToList() ?? new List<Transportation>();

			// If there are no values ​​for startup and transportation,
			// the total price of the tour is returned as 0
			if (activeTourTrips.Count == 0 && activeTransportations.Count == 0)
			{
				tour.TotalPrice = 0;
				tour.LastUpdatedDate = DateTime.Now;
				await _iUnitOfWork.SaveChangesAsync();
				return;
			}

			// Calculated a new value if have enough value
			decimal averageTourTripPrice = activeTourTrips.Average(tt => tt.Price);
			decimal totalTransportationPrice = activeTransportations.Sum(t => t.Price);

			// Set total price as average of tour trips plus total transportation cost
			tour.TotalPrice = averageTourTripPrice + totalTransportationPrice;
			tour.LastUpdatedDate = DateTime.Now;

			await _iUnitOfWork.SaveChangesAsync();
		}

		#region Helper method for TourService
		private async Task<Tour> GetTourById(Guid id)
		{
			return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetQueryable()
				.Include(t => t.TourMoods)
				.Include(t => t.LocationInTours)
				.SingleOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
		}

		private async Task<Tour> CreateTourFromRequest(TourRequestModel request, ApplicationUser user)
		{
			var tour = _mapper.Map<Tour>(request);
			SetInitialTourProperties(tour, user);
			ConfigureRelationships(tour, request);
			return tour;
		}

		private void SetInitialTourProperties(Tour tour, ApplicationUser user)
		{
			tour.Id = Guid.NewGuid();
			tour.Code = GenerateUniqueCode();
			tour.UserId = user.Id;
			tour.CreatedBy = user.UserName;
			tour.CreatedDate = DateTime.UtcNow;
			tour.LastUpdatedBy = user.UserName;
			tour.LastUpdatedDate = DateTime.UtcNow;
		}

		private async Task UpdateTourFromRequest(Tour tour, TourRequestModel request, ApplicationUser user)
		{
			UpdateTourProperties(tour, request, user);

			// Remove old relationships using DeleteRange
			foreach (var tourMood in tour.TourMoods)
			{
				_iUnitOfWork.GetRepositoryByEntity<TourMood>().Delete(tourMood);
			}
			foreach (var locationInTour in tour.LocationInTours)
			{
				_iUnitOfWork.GetRepositoryByEntity<LocationInTour>().Delete(locationInTour);
			}

			// Create new relationships
			var newTourMoods = CreateTourMoods(tour.Id, request.TourMoods, user);
			var newLocationInTours = CreateLocationInTours(tour.Id, request.LocationInTours, user);

			// Add new relationships using AddRange
			foreach (var tourMood in newTourMoods)
			{
				await _iUnitOfWork.GetRepositoryByEntity<TourMood>().AddAsync(tourMood);
			}
			foreach (var locationInTour in newLocationInTours)
			{
				await _iUnitOfWork.GetRepositoryByEntity<LocationInTour>().AddAsync(locationInTour);
			}

			// Update the navigation properties
			tour.TourMoods = newTourMoods;
			tour.LocationInTours = newLocationInTours;
		}

		private void UpdateTourProperties(Tour tour, TourRequestModel request, ApplicationUser user)
		{
			tour.Title = request.Title;
			tour.Description = request.Description;
			tour.Status = request.Status;
			tour.StartDate = request.StartDate;
			tour.EndDate = request.EndDate;
			tour.LastUpdatedBy = user.UserName;
			tour.LastUpdatedDate = DateTime.Now;
		}

		private List<TourMood> CreateTourMoods(Guid tourId, List<Guid>? moodIds, ApplicationUser user)
		{
			return moodIds?.Select(moodId => new TourMood
			{
				Id = Guid.NewGuid(),
				Code = GenerateUniqueCode(),
				TourId = tourId,
				MoodId = moodId,
				CreatedBy = user.UserName,
				CreatedDate = DateTime.Now,
				LastUpdatedBy = user.UserName,
				LastUpdatedDate = DateTime.Now
			}).ToList() ?? new List<TourMood>();
		}

		private List<LocationInTour> CreateLocationInTours(Guid tourId, List<Guid>? locationIds, ApplicationUser user)
		{
			return locationIds?.Select((locationId, index) => new LocationInTour
			{
				Id = Guid.NewGuid(),
				Code = GenerateUniqueCode(),
				TourId = tourId,
				LocationId = locationId,
				CreatedBy = user.UserName,
				CreatedDate = DateTime.Now,
				LastUpdatedBy = user.UserName,
				LastUpdatedDate = DateTime.Now
			}).ToList() ?? new List<LocationInTour>();
		}

		private async Task ValidateTourRequest(TourRequestModel request)
		{
			var errors = new List<string>();

			// Validate TourMoods and LocationInTours
			if (request.TourMoods?.Count > 0)
			{
				var existingMoods = await _iUnitOfWork.GetRepositoryByEntity<Moods>().GetQueryable()
					.Where(m => request.TourMoods.Contains(m.Id))
					.Select(m => m.Id)
					.ToListAsync();

				var invalidMoods = request.TourMoods.Except(existingMoods);
				if (invalidMoods.Any())
					errors.Add($"Invalid mood IDs: {string.Join(", ", invalidMoods)}");
			}

			if (request.LocationInTours?.Count > 0)
			{
				var existingLocations = await _iUnitOfWork.GetRepositoryByEntity<Location>().GetQueryable()
					.Where(l => request.LocationInTours.Contains(l.Id))
					.Select(l => l.Id)
					.ToListAsync();

				var invalidLocations = request.LocationInTours.Except(existingLocations);
				if (invalidLocations.Any())
					errors.Add($"Invalid location IDs: {string.Join(", ", invalidLocations)}");
			}
		}

		private void ConfigureRelationships(Tour tour, TourRequestModel request)
		{
			var user = new { UserName = tour.CreatedBy };
			tour.TourMoods = CreateTourMoods(tour.Id, request.TourMoods, new ApplicationUser { UserName = user.UserName });
			tour.LocationInTours = CreateLocationInTours(tour.Id, request.LocationInTours, new ApplicationUser { UserName = user.UserName });

			tour.TourTrips = new List<TourTrip>();
			tour.TourTimestamps = new List<TourTimestamp>();
			tour.Transportations = new List<Transportation>();
		}

		private async Task UpdateRelationships(Tour tour, TourRequestModel request)
		{
			// Remove existing relationships
			_iUnitOfWork.GetRepositoryByEntity<TourMood>().DeleteRange(tour.TourMoods);
			_iUnitOfWork.GetRepositoryByEntity<LocationInTour>().DeleteRange(tour.LocationInTours);

			// Add new relationships
			ConfigureRelationships(tour, request);
		}

		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

		private async Task<ApplicationUser> GetAuthenticatedUserAsync()
		{
			var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);
			if (user == null)
			{
				throw new UnauthorizedAccessException("User not authenticated");
			}
			return user;
		}
		#endregion
	}
}
