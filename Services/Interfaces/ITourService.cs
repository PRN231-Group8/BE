using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface ITourService
    {
        Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm);
        Task<IList<Tour>> GetAll();
        Task<Tour> GetById(Guid id);
        Task Add(TourRequestModel tour);
        Task<Tour> Update(TourRequestModel tour,Guid id);
        Task Delete(Guid id);
    }
}
