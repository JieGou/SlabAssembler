namespace Urbbox.SlabAssembler.Repositories.Core
{
    public interface IOperableByTransaction<in TElement> where TElement : class
    {
        ITransaction<TElement> StartTransaction();
    }
}
