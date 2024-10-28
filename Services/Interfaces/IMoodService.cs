using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Interfaces
{
    public interface IMoodService
    {
        Task<List<Moods>> GetAllAsync(int page, int pageSize, List<string>? searchTerm);
        Task<Moods> GetById(Guid id);
        Task Add(MoodRequest moods);
        Task Update(MoodRequest moods, Guid id);
        Task Delete(Guid id);
    }
}
