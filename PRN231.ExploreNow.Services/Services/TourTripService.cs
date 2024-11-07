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
	public class TourTripService : ITourTripService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IMapper _mapper;
		private readonly ITourService _tourService;

		public TourTripService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
			IHttpContextAccessor httpContextAccessor, IMapper mapper, ITourService tourService)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_httpContextAccessor = httpContextAccessor;
			_mapper = mapper;
			_tourService = tourService;
		}

		public async Task<(List<TourTripResponse> Items, int TotalCount)> GetAllTourTripAsync(int page, int pageSize, bool? sortByPrice, string? searchTerm)
		{
			return await _unitOfWork.GetRepository<ITourTripRepository>().GetAllTourTripAsync(page, pageSize, sortByPrice, searchTerm);
		}

		public async Task<TourTripResponse> GetTourTripByIdAsync(Guid tourTripId)
		{
			var tourTrip = await _unitOfWork.GetRepository<ITourTripRepository>().GetQueryable()
						  .Where(l => l.Id == tourTripId && !l.IsDeleted && !l.Tour.IsDeleted)
						  .Include(t => t.Tour)
						  .SingleOrDefaultAsync();

			return _mapper.Map<TourTripResponse>(tourTrip);
		}

		public async Task<TourDetailsResponse> GetTourTripsByTourIdAsync(Guid tourId)
		{
			var tour = await _unitOfWork.GetRepository<ITourRepository>().GetQueryable()
					  .Where(t => t.Id == tourId && !t.IsDeleted)
					  .Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
					  .SingleOrDefaultAsync();

			return _mapper.Map<TourDetailsResponse>(tour);
		}

		public async Task<List<TourTripResponse>> CreateBatchTourTrips(List<TourTripRequest> tourTripRequests)
		{
			var user = await GetAuthenticatedUserAsync();
			var tourTripEntities = _mapper.Map<List<TourTrip>>(tourTripRequests);

			// Group trips by TourId
			var groupedTrips = tourTripEntities.GroupBy(t => t.TourId);
			var processedTourIds = new HashSet<Guid>();

			foreach (var group in groupedTrips)
			{
				var tourId = group.Key;
				var tripsForTour = group.ToList();
				processedTourIds.Add(tourId);

				// Validate tour existence and get tour details for date range validation
				var tour = await _unitOfWork.GetRepository<ITourRepository>()
					.GetQueryable()
					.Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
					.FirstOrDefaultAsync(t => t.Id == tourId && !t.IsDeleted);

				if (tour == null)
					throw new InvalidOperationException($"Tour with ID {tourId} not found");

				int totalNewSeats = tripsForTour.Sum(t => t.TotalSeats);

				bool isValid = await ValidateTotalSeatsTransportation(tourId, totalNewSeats);

				if (!isValid)
				{
					throw new InvalidOperationException(
						$"Total seats of all new tour trips ({totalNewSeats}) would exceed transportation capacity for tour {tourId}");
				}

				// Validate all trip dates against tour date range
				foreach (var trip in tripsForTour)
				{
					ValidateInitialTripStatus(trip.TripStatus);
					ValidateTripDate(trip.TripDate, tour);
				}

				// Get existing trips for this tour
				var existingTrips = await _unitOfWork.GetRepository<ITourTripRepository>().GetQueryable()
					.Where(t => t.TourId == tourId && !t.IsDeleted)
					.ToListAsync();

				// Check for overlaps within new trips and with existing trips
				var allTrips = existingTrips.Concat(tripsForTour).ToList();
				var overlaps = FindOverlaps(allTrips);
				if (overlaps.Count > 0)
				{
					var overlapDescriptions = overlaps.Select(o =>
						$"Trip date overlap detected: {o.Item1.TripDate:d} conflicts with {o.Item2.TripDate:d} for tour {tourId}");
					throw new InvalidOperationException($"Date overlaps detected: {string.Join(", ", overlapDescriptions)}");
				}

				// Set additional properties for new trips
				foreach (var trip in tripsForTour)
				{
					trip.Code = GenerateUniqueCode();
					trip.CreatedBy = user.UserName;
					trip.CreatedDate = DateTime.Now;
					trip.LastUpdatedDate = DateTime.Now;
					trip.LastUpdatedBy = user.UserName;
					trip.TripStatus = TripStatus.OPEN;
					trip.BookedSeats = 0;
				}
			}

			await _unitOfWork.GetRepository<ITourTripRepository>().AddRangeAsync(tourTripEntities);
			await _unitOfWork.SaveChangesAsync();

			// Update totalprice for tour after create a specific or a list tourtrip
			foreach (var tourId in processedTourIds)
			{
				var tour = await _unitOfWork.GetRepository<ITourRepository>()
					.GetQueryable()
					.Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
					.FirstOrDefaultAsync(t => t.Id == tourId && !t.IsDeleted);

				if (tour != null && tour.Transportations.Count > 0)
				{
					await _tourService.UpdateTourPrice(tourId);
				}
			}

			return _mapper.Map<List<TourTripResponse>>(tourTripEntities);
		}

		public async Task<TourTripResponse> UpdateTourTrip(Guid tourTripId, TourTripRequest updateRequest)
		{
			var user = await GetAuthenticatedUserAsync();
			var existingTourTrip = await _unitOfWork.GetRepository<ITourTripRepository>().GetQueryable()
								  .Include(tt => tt.Tour)
								  .Include(tt => tt.Payments)
								  .SingleOrDefaultAsync(tt => tt.Id == tourTripId && !tt.IsDeleted);

			// Validate tour existence
			var tour = await _unitOfWork.GetRepository<ITourRepository>().GetQueryable()
					  .FirstOrDefaultAsync(t => t.Id == updateRequest.TourId && !t.IsDeleted);

			if (tour == null)
				throw new InvalidOperationException($"Tour with ID {updateRequest.TourId} not found");

			// Status transition validation
			await ValidateStatusTransition(existingTourTrip, updateRequest.TripStatus);

			// Date validation
			if (existingTourTrip.TripDate != updateRequest.TripDate)
			{
				ValidateTripDate(updateRequest.TripDate, tour);

				// Check overlaps with other trips
				var otherTrips = await _unitOfWork.GetRepository<ITourTripRepository>().GetQueryable()
								.Where(tt => tt.TourId == updateRequest.TourId && tt.Id != tourTripId && !tt.IsDeleted)
								.ToListAsync();

				if (otherTrips.Any(tt => IsTimeOverlapping(tt.TripDate, updateRequest.TripDate)))
					throw new InvalidOperationException($"Trip date {updateRequest.TripDate:d} overlaps with an existing trip");
			}

			// Seats validation
			if (updateRequest.TotalSeats < existingTourTrip.BookedSeats)
				throw new InvalidOperationException("Cannot reduce total seats below current booked seats");

			if (updateRequest.TotalSeats > existingTourTrip.TotalSeats)
			{
				// Calculate the additional number of seats
				int additionalSeats = updateRequest.TotalSeats - existingTourTrip.TotalSeats;

				bool isValid = await ValidateTotalSeatsTransportation(
					existingTourTrip.TourId,
					additionalSeats  // Only transmit the increased number of seats
				);

				if (!isValid)
				{
					throw new InvalidOperationException(
						$"Cannot update total seats to {updateRequest.TotalSeats}. " +
						"The new total would exceed transportation capacity.");
				}
			}

			// Handle status-specific logic
			await HandleStatusUpdate(existingTourTrip, updateRequest.TripStatus, user);

			// Update properties
			existingTourTrip.TripDate = updateRequest.TripDate;
			existingTourTrip.Price = updateRequest.Price;
			existingTourTrip.TotalSeats = updateRequest.TotalSeats;
			existingTourTrip.TripStatus = updateRequest.TripStatus;
			existingTourTrip.LastUpdatedDate = DateTime.Now;
			existingTourTrip.LastUpdatedBy = user.UserName;

			await _unitOfWork.SaveChangesAsync();
			await _tourService.UpdateTourPrice(existingTourTrip.TourId);

			return _mapper.Map<TourTripResponse>(existingTourTrip);
		}

		public async Task<bool> DeleteTourTrip(Guid id)
		{
			var tourTrip = await _unitOfWork.GetRepository<ITourTripRepository>()
				.GetQueryable()
				.FirstOrDefaultAsync(tt => tt.Id == id);

			var tourId = tourTrip.TourId;
			var result = await _unitOfWork.GetRepository<ITourTripRepository>().DeleteAsync(id);
			await _unitOfWork.SaveChangesAsync();

			if (result)
			{
				await _tourService.UpdateTourPrice(tourId);
			}

			return result;
		}

		public async Task<bool> ValidateTotalSeatsTransportation(Guid tourId, int newTotalSeats)
		{
			var tour = await _unitOfWork.GetRepository<ITourRepository>().GetQueryable()
				.Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
				.Include(t => t.Transportations.Where(tr => !tr.IsDeleted))
				.FirstOrDefaultAsync(t => t.Id == tourId);

			if (tour == null) return false;

			if (!tour.Transportations.Any())
				return true;

			// Calculate the current total number of seats for all TourTrips
			int currentTotalSeats = tour.TourTrips.Sum(tt => tt.TotalSeats);

			// Find the transportation with the smallest capacity
			int minTransportationCapacity = tour.Transportations.Min(t => t.Capacity);

			// Check if the total number of seats (including new seats) does not exceed capacity
			return (currentTotalSeats + newTotalSeats) <= minTransportationCapacity;
		}

		#region Helper method TourTripService
		private async Task ValidateStatusTransition(TourTrip tourTrip, TripStatus newStatus)
		{
			// Cannot change status if trip has passed unless completing it
			if (tourTrip.TripDate < DateTime.Now && newStatus != TripStatus.COMPLETED)
				throw new InvalidOperationException("Cannot modify status of past trips except to complete them");

			// Validate based on current status
			switch (tourTrip.TripStatus)
			{
				case TripStatus.COMPLETED:
					throw new InvalidOperationException("Cannot change status of completed tour trip");

				case TripStatus.CANCELLED:
					if (newStatus != TripStatus.OPEN)
						throw new InvalidOperationException("Cancelled trip can only be reopened");
					break;

				case TripStatus.FULLYBOOKED:
					if (newStatus == TripStatus.OPEN)
					{
						if (tourTrip.BookedSeats >= tourTrip.TotalSeats)
							throw new InvalidOperationException("Cannot reopen fully booked trip - all seats are still booked");
					}
					break;
			}

			// Validate based on new status
			switch (newStatus)
			{
				case TripStatus.COMPLETED:
					if (tourTrip.TripDate > DateTime.Now)
						throw new InvalidOperationException("Cannot complete future trip");

					var pendingPayments = tourTrip.Payments?
						.Where(p => p.Status == PaymentStatus.PENDING)
						.ToList();

					if (pendingPayments?.Count > 0)
					{
						throw new InvalidOperationException(
							$"Cannot complete trip with {pendingPayments.Count} pending payments. Please process all payments first.");
					}

					var completedPaymentsCount = tourTrip.Payments?
						.Count(p => p.Status == PaymentStatus.COMPLETED) ?? 0;

					if (completedPaymentsCount < tourTrip.TotalSeats)
					{
						var completionPercentage = (completedPaymentsCount * 100.0) / tourTrip.TotalSeats;
						throw new InvalidOperationException(
							$"Cannot complete trip - Only {completionPercentage:F1}% of seats have completed payments ({completedPaymentsCount}/{tourTrip.TotalSeats} seats)");
					}
					break;

				case TripStatus.CANCELLED:
					if (tourTrip.TripDate <= DateTime.Now)
						throw new InvalidOperationException("Cannot cancel past trip");

					var completedPayments = tourTrip.Payments?.Any(p => p.Status == PaymentStatus.COMPLETED);
					if (completedPayments == true)
						throw new InvalidOperationException("Cannot cancel trip with completed payments");
					break;

				case TripStatus.OPEN:
					if (tourTrip.BookedSeats >= tourTrip.TotalSeats)
						throw new InvalidOperationException("Cannot reopen trip - all seats are booked");
					break;
			}
		}

		private async Task HandleStatusUpdate(TourTrip tourTrip, TripStatus newStatus, ApplicationUser user)
		{
			switch (newStatus)
			{
				case TripStatus.COMPLETED:
					break;

				case TripStatus.CANCELLED:
					// Update any pending payments to cancelled
					if (tourTrip.Payments != null)
					{
						foreach (var payment in tourTrip.Payments.Where(p => p.Status == PaymentStatus.PENDING))
						{
							payment.Status = PaymentStatus.CANCELLED;
							payment.LastUpdatedBy = user.UserName;
							payment.LastUpdatedDate = DateTime.Now;
						}
					}
					break;

				case TripStatus.OPEN:
					break;

				case TripStatus.FULLYBOOKED:
					break;
			}
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

		private static List<(TourTrip, TourTrip)> FindOverlaps(List<TourTrip> trips)
		{
			return trips
				.SelectMany((t1, i) => trips.Skip(i + 1), (t1, t2) => (t1, t2))
				.Where(t => IsTimeOverlapping(t.t1.TripDate, t.t2.TripDate))
				.ToList();
		}

		private void ValidateTripDate(DateTime tripDate, Tour tour)
		{
			if (!tour.StartDate.HasValue || !tour.EndDate.HasValue)
				return;

			if (tripDate.Date < tour.StartDate.Value.Date)
				throw new InvalidOperationException(
					$"Trip date {tripDate:d} is before tour start date {tour.StartDate.Value:d}");

			if (tripDate.Date > tour.EndDate.Value.Date)
				throw new InvalidOperationException(
					$"Trip date {tripDate:d} is after tour end date {tour.EndDate.Value:d}");
		}

		private void ValidateInitialTripStatus(TripStatus status)
		{
			switch (status)
			{
				case TripStatus.COMPLETED:
					throw new InvalidOperationException("Cannot create a new tour trip with COMPLETED status");
				case TripStatus.CANCELLED:
					throw new InvalidOperationException("Cannot create a new tour trip with CANCELLED status");
				case TripStatus.FULLYBOOKED:
					throw new InvalidOperationException("New tour trip must start with OPEN status");
			}
		}

		private static bool IsTimeOverlapping(DateTime date1, DateTime date2)
		{
			return date1.Date == date2.Date;
		}
		#endregion
	}
}
