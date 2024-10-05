using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories
{
    public class TourRepository : BaseRepository<Tour>, ITourRepository
    {
        private readonly BaseRepository<Tour> _baseRepository;

        public TourRepository(DbContext dbContext) : base(dbContext)
        {
            _baseRepository = new BaseRepository<Tour>(dbContext);
        }

    }
}
