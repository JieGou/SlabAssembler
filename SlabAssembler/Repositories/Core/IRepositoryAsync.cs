using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Urbbox.SlabAssembler.Repositories.Core
{
    public interface IRepositoryAsync<TEntity> where TEntity : class, IEntity
    {
        Task<TEntity> GetAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Func<TEntity, bool> predicate);

        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        Task RemoveAsync(TEntity entity);
        Task RemoveRangeAsync(IEnumerable<TEntity> entities);
    }
}
