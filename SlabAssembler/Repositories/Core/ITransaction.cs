using System;

namespace Urbbox.SlabAssembler.Repositories.Core
{
    public interface ITransaction<in TElement> : IDisposable where TElement : class
    {
        void Remove(TElement element);
        void Insert(TElement element);
        void Commit();
        void Rollback();
    }
}
