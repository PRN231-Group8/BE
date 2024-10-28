using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System.Security.Claims;

namespace PRN231.ExploreNow.Services.Services
{
	public class TourService : ITourService
	{
		private IUnitOfWork _iUnitOfWork;
		private IHttpContextAccessor _iContextAccessor;
		private UserManager<ApplicationUser> _userManager;

		public TourService(IUnitOfWork iUnitOfWork, IHttpContextAccessor IContextAccessor, UserManager<ApplicationUser> userManager)
		{
			_iUnitOfWork = iUnitOfWork;
			_iContextAccessor = IContextAccessor;
			_userManager = userManager;
		}

		public async Task Add(TourRequestModel tour)
		{
			var _tour = MapToTour(tour);
			await _iUnitOfWork.GetRepositoryByEntity<Tour>().AddAsync(_tour);
			await _iUnitOfWork.SaveChangesAsync();
		}

		public async Task Delete(Guid id)
		{
			await _iUnitOfWork.GetRepositoryByEntity<Tour>().DeleteAsync(id);
		}

		public async Task<IList<Tour>> GetAll()
		{
			return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetAll();
		}

		public async Task<Tour> GetById(Guid id)
		{
			return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetById(id);
		}

		public async Task<List<Tour>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, string? searchTerm)
		{
			return await _iUnitOfWork.TourRepository.GetToursAsync(page, pageSize, sortByStatus, searchTerm);
		}

		public async Task UpdateAsync(TourRequestModel tour, Guid id)
		{
			var _tour = MapToTour(tour);
			_tour.Id = id;
			await _iUnitOfWork.GetRepositoryByEntity<Tour>().UpdateAsync(_tour);
			await _iUnitOfWork.SaveChangesAsync();
		}

		private Tour MapToTour(TourRequestModel tour)
		{
			var currentUser = _iContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var currUserName = _iContextAccessor.HttpContext?.User.Identity.Name;
			return new Tour
			{
				Code = tour.Code,
				CreatedBy = currUserName,
				CreatedDate = DateTime.Now,
				StartDate = tour.StartDate,
				EndDate = tour.EndDate,
				LastUpdatedBy = currUserName,
				LastUpdatedDate = DateTime.Now,
				IsDeleted = false,
				TotalPrice = tour.TotalPrice,
				Status = tour.Status,
				UserId = currentUser,
				Title = tour.Title,
				Description = tour.Description,
			};
		}
	}
}
