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
            if (!File.Exists(datafile))
            {
                using (File.Create(datafile)) { }
                Save();
            }

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
                base.Remove(entity);
                Save();
            }
        }

        private void Save()
        {
            var serializer = new XmlSerializer(typeof(List<TEntity>));
            using (TextWriter writer = new StreamWriter(RepositoryDataFile))
                serializer.Serialize(writer, Entities);
        }

        private void Load()
        {
            List<TEntity> entities;
            var deserializer = new XmlSerializer(typeof(List<TEntity>));
            using (TextReader reader = new StreamReader(RepositoryDataFile))
                entities = (List<TEntity>) deserializer.Deserialize(reader);

            Entities.Clear();
            Entities.AddRange(entities);
        }

        public sealed class XMLRepositoryTransaction : ITransaction<TEntity>
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
                _xmlRepository.Entities.AddRange(_addedEntities);
                foreach (var entity in _removedEntities)
                    _xmlRepository.Entities.Remove(entity);

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
