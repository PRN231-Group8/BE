using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;

namespace PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IUserRepository UserRepository { get; }
}