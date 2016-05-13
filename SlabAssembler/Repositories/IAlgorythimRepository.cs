using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Repositories
{
    public interface IAlgorythimRepository
    {
        void SetOptions(AssemblyOptions options);
        AssemblyOptions GetDefaultOptions();
    }
}
