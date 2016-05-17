using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Repositories
{
    public interface IAlgorythimRepository
    {
        AssemblyOptions GetAssemblyOptions();
        void SetAssemblyOptions(AssemblyOptions options);
        void ResetAssemblyOptions();
    }
}
