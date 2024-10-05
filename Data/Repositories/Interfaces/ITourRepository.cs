using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Interfaces
{
    public interface ITourRepository : IBaseRepository<Tour>
    {
        Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm);
    }
}
