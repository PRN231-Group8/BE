using PRN231.ExploreNow.Repositories.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks
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
