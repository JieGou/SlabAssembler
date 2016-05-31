using Urbbox.SlabAssembler.Core.Models;
using Urbbox.SlabAssembler.Repositories.Core;

namespace Urbbox.SlabAssembler.Repositories
{
    public interface IAlgorythimRepository : IRepository<AssemblyOptions>, IOperableByTransaction<AssemblyOptions>
    {
        AssemblyOptions Get();
        void SaveChanges();
        void Reset();
    }
}
