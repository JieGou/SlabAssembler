using Autodesk.AutoCAD.ApplicationServices;
using System;
using Urbbox.SlabAssembler.Core.Strategies;
using Urbbox.SlabAssembler.Repositories;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.SlabAssembler.Core.Assemblers
{
    public abstract class SlabAssembler : Builder, IDisposable
    {
        protected SlabProperties Properties { get; }
        protected AcEnvironment Environment { get; }
        protected IPartRepository PartRepository { get; }
        protected Document Document { get; }
        private DocumentLock _lock;

        public SlabAssembler(IPartRepository partRepository, SlabProperties properties)
        {
            Document = AcApplication.DocumentManager.MdiActiveDocument;
            Properties = properties;
            PartRepository = partRepository;
            Environment = new AcEnvironment(properties.Parts.SelectedOutline)
            {
                GirdersLayer = properties.Parts.SelectedGirdersLayer,
                CollumnsLayer = properties.Parts.SelectedColumnsLayer,
                EmptiesLayer = properties.Parts.SelectedEmptiesLayer,
            };

            _lock = Document.LockDocument();
        }

        protected abstract override IStrategy NextStrategy { get; }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
