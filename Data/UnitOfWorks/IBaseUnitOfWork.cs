using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;

namespace PRN231.ExploreNow.Repositories.UnitOfWorks;

public interface IBaseUnitOfWork : IDisposable
{
    IBaseRepository<TEntity> GetRepositoryByEntity<TEntity>() where TEntity : BaseEntity;

    TRepository GetRepository<TRepository>() where TRepository : IBaseRepository;

    Task<bool> SaveChanges(CancellationToken cancellationToken = default);
}