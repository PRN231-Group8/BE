using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface ITourService
    {
        Task<List<TourResponse>> GetToursAsync(int page, int pageSize, TourStatus? sortByStatus, List<string>? searchTerm);
        Task<TourResponse> GetById(Guid id);
        Task<TourResponse> Add(TourRequestModel tour);
        Task<TourResponse> UpdateAsync(TourRequestModel tour, Guid id);
        Task Delete(Guid id);
    }
}
