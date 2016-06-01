using System;
using System.Collections.Generic;
using System.Linq;

namespace Urbbox.SlabAssembler.Repositories.Core
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected List<TEntity> Entities { get; }

        public Repository()
        {
            Entities = new List<TEntity>();
        }

        public TEntity Get(int pos)
        {
            return Entities[pos];
        }

        public IEnumerable<TEntity> GetAll()
        {
            return Entities;
        }

        public IEnumerable<TEntity> Find(Func<TEntity, bool> predicate)
        {
            return Entities.Where(predicate);
        }

        public virtual void Add(TEntity entity)
        {
            Entities.Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
                Add(e);
        }

        public void RemoveAt(int pos)
        {
            Entities.RemoveAt(pos);
        }

        public virtual void Remove(TEntity entity)
        {
            Entities.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
                Remove(entity);
        }

        public void Clear()
        {
            Entities.Clear();
        }
    }
}
