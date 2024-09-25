using ExploreNow.Domain.Contracts.Repositories;
using ExploreNow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreNow.Domain.Contracts.UnitOfWorks
{
    public interface IBaseUnitOfWork : IDisposable
    {
        IBaseRepository<TEntity> GetRepositoryByEntity<TEntity>() where TEntity : BaseEntity;

        TRepository GetRepository<TRepository>() where TRepository : IBaseRepository;

        Task<bool> SaveChanges(CancellationToken cancellationToken = default);
    }
}
