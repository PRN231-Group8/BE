using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface ITourService
    {

        Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm);
        Task<IList<Tour>> GetAll();
        Task<Tour> GetById(Guid id);
        Task Add(Tour tour);
        Task<Tour> Update(Tour tour);
        Task Delete(Guid id);
    }
}
