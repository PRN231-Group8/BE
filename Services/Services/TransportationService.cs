using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System.Linq.Expressions;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
	public class TransportationService : ITransportationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ITourService _tourService;

		public TransportationService(IUnitOfWork unitOfWork, IMapper mapper, ITourService tourService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_tourService = tourService;
		}

		public async Task<TransportationResponse> GetTransportationById(Guid id)
		{
			var transportation = await _unitOfWork.GetRepository<ITransportationRepository>().GetById(id);
			return transportation != null ? _mapper.Map<TransportationResponse>(transportation) : null;
		}

		public async Task<(List<TransportationResponse> Items, int TotalElements)> GetTransportations(int page, int pageSize, string? sortBy, string? searchTerm)
		{
			var queryable = _unitOfWork.GetRepository<ITransportationRepository>().GetQueryable().Where(t => !t.IsDeleted);
			var totalElements = await _unitOfWork.GetRepository<ITransportationRepository>().GetTotalCount();

			// Apply search filter if searchTerm is provided
			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				queryable = queryable.Where(t => t.Type.ToString().Contains(searchTerm) || t.Capacity.ToString().Contains(searchTerm));
			}

			// Apply multiple field sorting
			if (!string.IsNullOrWhiteSpace(sortBy))
			{
				queryable = ApplySorting(queryable, sortBy);
			}
			else
			{
				queryable = queryable.OrderBy(t => t.Id); // Default sort if no sortBy provided
			}

			// Apply pagination
			var paginatedList = await queryable
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var mappedTransportations = _mapper.Map<List<TransportationResponse>>(paginatedList);
			return (mappedTransportations, totalElements);
		}

		public async Task<bool> AddTransportation(TransportationRequestModel req)
		{
			var transportation = _mapper.Map<Transportation>(req);

			var tour = await _unitOfWork.GetRepository<ITourRepository>().GetQueryable()
				.Include(t => t.TourTrips.Where(tt => !tt.IsDeleted))
				.SingleOrDefaultAsync(t => t.Id == req.TourId && !t.IsDeleted);
			if (tour == null)
				return false;

			if (tour.TourTrips.Any())
			{
				int totalTourSeats = tour.TourTrips.Sum(tt => tt.TotalSeats);
				if (totalTourSeats > req.Capacity)
				{
					throw new InvalidOperationException(
						$"Cannot add transportation with capacity {req.Capacity}. " +
						$"Total seats of all tour trips ({totalTourSeats}) exceeds transportation capacity.");
				}
			}

			ApplicationUser currentUser = await _unitOfWork.GetRepository<IUserRepository>().GetUsersClaimIdentity();

			transportation.Code = GenerateUniqueCode();
			transportation.CreatedBy = currentUser.UserName;
			transportation.StartDate = DateTime.Now;
			transportation.LastUpdatedBy = currentUser.UserName;
			transportation.LastUpdatedDate = DateTime.Now;
			transportation.IsDeleted = false;
			transportation.Tour = tour;

			await _unitOfWork.GetRepository<ITransportationRepository>().AddAsync(transportation);
			var result = await _unitOfWork.SaveChangesAsync();

			await _tourService.UpdateTourPrice(req.TourId);
			return result;
		}

		public async Task<bool> UpdateTransportation(Guid id, TransportationRequestModel req)
		{
			var existingTransportation = await _unitOfWork.GetRepository<ITransportationRepository>()
				.GetQueryable()
				.Include(t => t.Tour)
					.ThenInclude(tour => tour.TourTrips.Where(tt => !tt.IsDeleted))
				.SingleOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
			if (existingTransportation == null)
				return false;

			int totalTourSeats = existingTransportation.Tour.TourTrips.Sum(tt => tt.TotalSeats);
			if (totalTourSeats > req.Capacity)
			{
				throw new InvalidOperationException(
					$"Cannot update transportation capacity to {req.Capacity}. " +
					$"Total seats of all tour trips ({totalTourSeats}) exceeds new capacity.");
			}

			_mapper.Map(req, existingTransportation);
			await _unitOfWork.GetRepository<ITransportationRepository>().UpdateAsync(existingTransportation);
			var result = await _unitOfWork.SaveChangesAsync();

			await _tourService.UpdateTourPrice(req.TourId);
			return result;
		}

		public async Task<bool> DeleteTransportation(Guid id)
		{
			var transportation = await _unitOfWork.GetRepository<ITransportationRepository>().GetById(id);
			if (transportation == null)
				return false;

			var tourId = transportation.TourId;

			var result = await _unitOfWork.GetRepository<ITransportationRepository>().DeleteAsync(id);

			if (result)
			{
				await _tourService.UpdateTourPrice(tourId);
			}

			return result;
		}

		#region Helper method
		private IQueryable<Transportation> ApplySorting(IQueryable<Transportation> query, string sortBy)
		{
			var isFirst = true;

			foreach (var sortOption in sortBy.Split(','))
			{
				var sortParams = sortOption.Split(':');
				var field = sortParams[0];
				var direction = sortParams.Length > 1 && sortParams[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
					? "OrderByDescending"
					: "OrderBy";

				// Use reflection to apply sorting by field name
				var parameter = Expression.Parameter(typeof(Transportation), "t");
				var property = Expression.Property(parameter, field);
				var lambda = Expression.Lambda(property, parameter);

				var method = typeof(Queryable).GetMethods().First(m => m.Name == direction && m.GetParameters().Length == 2)
					.MakeGenericMethod(typeof(Transportation), property.Type);

				query = (IQueryable<Transportation>)method.Invoke(null, new object[] { query, lambda });
				isFirst = false;
			}

			return query;
		}

		private static string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
		#endregion
	}
}
