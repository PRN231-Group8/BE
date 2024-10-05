using Microsoft.VisualBasic;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using PRN231.ExploreNow.Repositories.UnitOfWorks;
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
        private UnitOfWork _unitOfWork;
        
        public TourService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Add(Tour tour)
        {
            _unitOfWork.GetRepositoryByEntity<Tour>().Add(tour);

            _unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            Tour tour = await _unitOfWork.GetRepositoryByEntity<Tour>().GetById(id);

            _unitOfWork.GetRepositoryByEntity<Tour>().Delete(tour);
            _unitOfWork.SaveChangesAsync();
        }

        public async Task<IList<Tour>> GetAll()
        {
            return await _unitOfWork.GetRepositoryByEntity<Tour>().GetAll();
        }

        public async Task<Tour> GetById(Guid id)
        {
            return await _unitOfWork.GetRepositoryByEntity<Tour>().GetById(id);
        }

        public async Task<Tour> Update(Tour tour)
        {
            _unitOfWork.GetRepositoryByEntity<Tour>().Update(tour);
            _unitOfWork.SaveChangesAsync();
            return tour;
        }
    }
}
