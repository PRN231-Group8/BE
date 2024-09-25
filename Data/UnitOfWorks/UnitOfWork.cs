using ExploreNow.Data.Context;
using ExploreNow.Domain.Contracts.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreNow.Data.UnitOfWorks
{
    public class UnitOfWork : BaseUnitOfWork<ApplicationDbContext>, IUnitOfWork
    {
        public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
        {
        }

        // public ILocationRepository LocationRepository => GetRepository<ILocationRepository>();
        // public ITourRepository TourRepository => GetRepository<ITourRepository>();
        // ...

    }
}
