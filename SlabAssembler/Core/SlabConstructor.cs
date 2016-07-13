using Autodesk.AutoCAD.ApplicationServices;
using System;
using Urbbox.SlabAssembler.Core.Strategies;
using Urbbox.SlabAssembler.Core.Strategies.LD;
using Urbbox.SlabAssembler.Repositories;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Urbbox.SlabAssembler.Core
{
    public class SlabConstructor : Builder, IDisposable
    {
        private SlabProperties _properties;
        private AcEnvironment _environment;
        private IPartRepository _partRepository;
        private Document _document;
        private DocumentLock _lock;

        public SlabConstructor(IPartRepository partRepository, SlabProperties properties)
        {
            _document = AcApplication.DocumentManager.MdiActiveDocument;
            _properties = properties;
            _partRepository = partRepository;
            _environment = new AcEnvironment(properties.Parts.SelectedOutline)
            {
                GirdersLayer = properties.Parts.SelectedGirdersLayer,
                CollumnsLayer = properties.Parts.SelectedColumnsLayer,
                EmptiesLayer = properties.Parts.SelectedEmptiesLayer,
            };

            _lock = _document.LockDocument();
        }

        protected override IStrategy NextStrategy
        {
            get
            {
                if (_properties.Algorythim.GlobalOrientationAngle == 0)
                { 
                    if (CurrentStrategy == null)
                        return new HorizontalLDStrategy(_properties, _partRepository, _environment);
                    else
                        return null;
                } else
                {
                    if (CurrentStrategy == null)
                        return new VerticalLDStrategy(_properties, _partRepository, _environment);
                    else
                        return null;
                }
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
