using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;

namespace PRN231.ExploreNow.Services.Services
{
	public class MoodService : IMoodService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _contextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;

		public MoodService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_contextAccessor = contextAccessor;
			_mapper = mapper;
		}

		public async Task<MoodResponse> Add(MoodRequest moods)
		{
			var user = await GetAuthenticatedUserAsync();
			var mood = _mapper.Map<Moods>(moods);

			mood.Code = GenerateUniqueCode();
			mood.CreatedBy = user.UserName;
			mood.CreatedDate = DateTime.Now;
			mood.LastUpdatedBy = user.UserName;

			await _unitOfWork.GetRepositoryByEntity<Moods>().AddAsync(mood);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<MoodResponse>(mood);
		}

		public async Task Delete(Guid id)
		{
			await _unitOfWork.GetRepositoryByEntity<Moods>().DeleteAsync(id);
		}

		public async Task<(List<MoodResponse> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? searchTerm)
		{
			var (moods, totalCount) = await _unitOfWork.GetRepository<IMoodRepository>().GetAllAsync(page, pageSize, searchTerm);
			var moodResponses = _mapper.Map<List<MoodResponse>>(moods);
			return (moodResponses, totalCount);
		}

		public async Task<MoodResponse> GetById(Guid id)
		{
			var mood = await _unitOfWork.GetRepositoryByEntity<Moods>().GetQueryable()
					  .Include(m => m.TourMoods)
					  .Where(m => m.Id == id && !m.IsDeleted)
					  .SingleOrDefaultAsync();

			return _mapper.Map<MoodResponse>(mood);
		}

		public async Task Update(MoodRequest moods, Guid id)
		{
			var user = await GetAuthenticatedUserAsync();
			var mood = _mapper.Map<Moods>(moods);

			mood.Id = id;
			mood.LastUpdatedBy = user.UserName;
			mood.LastUpdatedDate = DateTime.Now;

			await _unitOfWork.GetRepositoryByEntity<Moods>().UpdateAsync(mood);
			await _unitOfWork.SaveChangesAsync();
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
	}
}
