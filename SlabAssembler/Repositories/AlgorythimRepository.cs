using System.Linq;
using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    public class AlgorythimRepository : XMLRepository<AssemblyOptions>, IAlgorythimRepository
    {
        public AlgorythimRepository() : base(Properties.Resources.OptionsDataFile) { }

        public AssemblyOptions Get()
        {
            StartTransaction();
            return Get(0);
        }

        public void SaveChanges()
        {
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            StartTransaction();
        }

        public void Reset()
        {
            var defaultRepo = new XMLRepository<AssemblyOptions>(Properties.Resources.DefaultOptionsDataFile);

            Clear();
            using (var t = StartTransaction())
            {
                if (!defaultRepo.GetAll().Any())
                    Add(new AssemblyOptions());
                else
                    AddRange(defaultRepo.GetAll());
                t.Commit();
            }

            StartTransaction();
        }
    }
}
