using PRN231.ExploreNow.Repositories.Repositories.Interface;

namespace PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;

public interface IUnitOfWork : IBaseUnitOfWork
{
    ILocationRepository LocationRepository { get; }
    // ITourRepository TourRepository { get; }
    // ...
}
