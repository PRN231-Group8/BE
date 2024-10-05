using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Contracts;
using PRN231.ExploreNow.Repositories.Repositories.Interfaces;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IUserRepository UserRepository { get; }

    ITourRepository TourRepository { get; }
}