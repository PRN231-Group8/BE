using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
using PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IUserRepository UserRepository { get; }
}