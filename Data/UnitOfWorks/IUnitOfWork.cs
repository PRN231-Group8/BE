
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks;

public interface IUnitOfWork : IBaseUnitOfWork
{
    ITourRepository tourRepository {  get; }
    
    // ILocationRepository LocationRepository { get; }
    // ITourRepository TourRepository { get; }
    // ...
}