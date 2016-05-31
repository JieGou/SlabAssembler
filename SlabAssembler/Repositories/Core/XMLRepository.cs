using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Urbbox.SlabAssembler.Repositories.Core
{
    public class XMLRepository<TEntity> : Repository<TEntity>, IOperableByTransaction<TEntity> where TEntity : class
    {
        public string RepositoryDataFile { get; protected set; }
        public ITransaction<TEntity> CurrentTransaction { get; private set; }

        public XMLRepository(string datafile)
        {
            RepositoryDataFile = datafile;
            Load();
        }

        public ITransaction<TEntity> StartTransaction()
        {
            CurrentTransaction = new XMLRepositoryTransaction(this);
            return CurrentTransaction;
        }

        public override void Add(TEntity entity)
        {
            if (CurrentTransaction != null)
                CurrentTransaction.Insert(entity);
            else { 
                base.Add(entity);
                Save();
            }
        }

        public override void Remove(TEntity entity)
        {
            if (CurrentTransaction != null)
                CurrentTransaction.Remove(entity);
            else
            {
                base.Add(entity);
                Save();
            }
        }

        private void Save()
        {
            var serializer = new XmlSerializer(typeof(IEnumerable<TEntity>));
            using (TextWriter writer = new StreamWriter(RepositoryDataFile))
                serializer.Serialize(writer, Entities);
        }

        private void Load()
        {
            IEnumerable<TEntity> entities;
            var deserializer = new XmlSerializer(typeof(IEnumerable<TEntity>));
            using (TextReader reader = new StreamReader(RepositoryDataFile))
                entities = (IEnumerable<TEntity>) deserializer.Deserialize(reader);

            Entities.Clear();
            Entities.AddRange(entities);
        }

        public class XMLRepositoryTransaction : ITransaction<TEntity>
        {
            private readonly XMLRepository<TEntity> _xmlRepository;
            private LinkedList<TEntity> _addedEntities;
            private LinkedList<TEntity> _removedEntities;

            public XMLRepositoryTransaction(XMLRepository<TEntity> xmlRepository)
            {
                _xmlRepository = xmlRepository;
                _addedEntities = new LinkedList<TEntity>();
                _removedEntities = new LinkedList<TEntity>();
            }

            public void Dispose()
            {
                _addedEntities = null;
                _removedEntities = null;
                _xmlRepository.CurrentTransaction = null;
            }

            public void Remove(TEntity element)
            {
                _removedEntities.AddFirst(element);
            }

            public void Insert(TEntity element)
            {
                _addedEntities.AddFirst(element);
            }

            public void Commit()
            {
                _xmlRepository.AddRange(_addedEntities);
                _xmlRepository.RemoveRange(_removedEntities);
                _xmlRepository.Save();
                Rollback();
            }

            public void Rollback()
            {
                _addedEntities.Clear();
                _removedEntities.Clear();
            }
        }
    }

}
