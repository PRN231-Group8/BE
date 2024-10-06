using Microsoft.VisualBasic;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Services
{
    public class TourService : ITourService
    {
        private IUnitOfWork _iUnitOfWork;

        public TourService(IUnitOfWork iUnitOfWork)
        {
            _iUnitOfWork = iUnitOfWork;
        }

        public async Task Add(Tour tour)
        {
            _iUnitOfWork.GetRepositoryByEntity<Tour>().Add(tour);

            _iUnitOfWork.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            Tour tour = await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetById(id);

            _iUnitOfWork.GetRepositoryByEntity<Tour>().Delete(tour);
            _iUnitOfWork.SaveChangesAsync();
        }

        public async Task<IList<Tour>> GetAll()
        {
            return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetAll();
        }

        public async Task<Tour> GetById(Guid id)
        {
            return await _iUnitOfWork.GetRepositoryByEntity<Tour>().GetById(id);
        }

        public async Task<List<Tour>> GetToursAsync(int page, int pageSize, BookingStatus? sortByStatus, string? searchTerm)
        {
            return await GetToursAsync(page, pageSize, sortByStatus, searchTerm);
        }

        public async Task<Tour> Update(Tour tour)
        {
            _iUnitOfWork.GetRepositoryByEntity<Tour>().Update(tour);
            _iUnitOfWork.SaveChangesAsync();
            return tour;
        }
    }
}
