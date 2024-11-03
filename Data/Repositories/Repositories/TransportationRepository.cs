using AutoMapper;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.Repositories.Context;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Repositories.Repositories.Repositories
{
    public class TransportationRepository : BaseRepository<Transportation>, ITransportationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TransportationRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}
