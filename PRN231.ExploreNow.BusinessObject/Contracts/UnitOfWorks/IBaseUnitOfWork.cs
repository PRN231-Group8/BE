using PRN231.ExploreNow.BusinessObject.Contracts.Repositories;
using PRN231.ExploreNow.BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.BusinessObject.Contracts.UnitOfWorks
{
    public interface IBaseUnitOfWork : IDisposable
    {
        IBaseRepository<TEntity> GetRepositoryByEntity<TEntity>() where TEntity : BaseEntity;

        TRepository GetRepository<TRepository>() where TRepository : IBaseRepository;

        Task<bool> SaveChanges(CancellationToken cancellationToken = default);
    }
}
