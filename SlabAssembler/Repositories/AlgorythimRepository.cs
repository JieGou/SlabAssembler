using System.Linq;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    class AlgorythimRepository : XMLRepository<AssemblyOptions>, IAlgorythimRepository
    {
        public AlgorythimRepository() : base(Properties.Resources.OptionsDataFile) { }

        public AssemblyOptions Get()
        {
            StartTransaction();
            return GetAll().First();
        }

        public void SaveChanges()
        {
            CurrentTransaction.Commit();
            StartTransaction();
        }

        public void Reset()
        {
            var defaultRepo = new AlgorythimRepository { RepositoryDataFile = Properties.Resources.DefaultOptionsDataFile };
            using (var t = StartTransaction())
            {
                Clear();
                AddRange(defaultRepo.GetAll());
                t.Commit();
            }
        }
    }
}
