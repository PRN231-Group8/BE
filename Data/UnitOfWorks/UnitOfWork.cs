using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;
using PRN231.ExploreNow.Repositories.Context;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks;

public class UnitOfWork : BaseUnitOfWork<ApplicationDbContext>, IUnitOfWork
{
    public UnitOfWork(ApplicationDbContext context, IServiceProvider serviceProvider) : base(context, serviceProvider)
    {
    }
    public IUserRepository UserRepository => GetRepository<IUserRepository>();


    // public ILocationRepository LocationRepository => GetRepository<ILocationRepository>();
    // public ITourRepository TourRepository => GetRepository<ITourRepository>();
    // ...
}