using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
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

		public TourTimeStampService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_mapper = mapper;
		}

		public async Task<List<TourTimeStampResponse>> GetAllTourTimeStampAsync(int page, int pageSize, TimeSpan? sortByTime, string? searchTerm)
		{
			return await _unitOfWork.TourTimeStampRepository.GetAllTourTimestampsAsync(page, pageSize, sortByTime, searchTerm);
		}

		public async Task<TourTimeStampResponse> GetTourTimeStampByIdAsync(Guid tourTimeStampId)
		{
			return await _unitOfWork.TourTimeStampRepository.GetByIdAsync(tourTimeStampId);
		}

		public async Task<List<TourTimeStampResponse>> CreateMultipleTourTimeStampsAsync(List<TourTimeStampRequest> tourTimeStampRequests)
		{
			var tourTimestamps = _mapper.Map<List<TourTimestamp>>(tourTimeStampRequests);

			foreach (var timestamp in tourTimestamps)
			{
				timestamp.Code = GenerateUniqueCode();
			}

			return await _unitOfWork.TourTimeStampRepository.CreateMultipleAsync(tourTimestamps);
		}

		public async Task<TourTimeStampResponse> UpdateTourTimeStampAsync(Guid tourTimeStampId, TourTimeStampRequest tourTimeStampRequest)
		{
			var tourTimeStamp = _mapper.Map<TourTimestamp>(tourTimeStampRequest);
			tourTimeStamp.Id = tourTimeStampId;
			return await _unitOfWork.TourTimeStampRepository.UpdateAsync(tourTimeStamp, tourTimeStampRequest);
		}

		public async Task<bool> DeleteAsync(Guid tourTimeStampId)
		{
			var existingTourTimeStamp = await _unitOfWork.TourTimeStampRepository.GetByIdAsync(tourTimeStampId);

			if (existingTourTimeStamp == null)
			{
				throw new InvalidOperationException("Existing tour timestamp not found.");
			}

			return await _unitOfWork.TourTimeStampRepository.DeleteAsync(tourTimeStampId);
		}

		// Generate random Code
		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
	}
}
