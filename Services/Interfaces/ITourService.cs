using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
