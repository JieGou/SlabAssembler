using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Urbbox.SlabAssembler.Managers;

namespace Urbbox.SlabAssembler.Core
{
    public class AcEnvironment
    {
        private string _girdersLayer;
        private List<Entity> _girders;

        private string _collumnsLayer;
        private List<Entity> _collumns;

        private string _emptiesLayer;
        private List<Polyline> _empties;

        private ObjectId _outlineId;
        private AutoCadManager _acad;

        public Polyline Outline { get; private set; }
        public IReadOnlyCollection<Entity> Girders => new ReadOnlyCollection<Entity>(_girders);
        public IReadOnlyCollection<Entity> Collumns => new ReadOnlyCollection<Entity>(_collumns);
        public IReadOnlyCollection<Polyline> Empties => new ReadOnlyCollection<Polyline>(_empties);

        public string GirdersLayer
        {
            get { return _girdersLayer; }
            set
            {
                _girdersLayer = value;

                if (!string.IsNullOrEmpty(_girdersLayer))
                    using (var tr = _acad.StartOpenCloseTransaction())
                        foreach (ObjectId objId in _acad.GetLayerObjects(_girdersLayer))
                            AddGirder(tr, objId);
            }
        }

        public string CollumnsLayer
        {
            get { return _collumnsLayer; }
            set
            {
                _collumnsLayer = value;

                if (!string.IsNullOrEmpty(_collumnsLayer))
                    using (var tr = _acad.StartOpenCloseTransaction())
                        foreach (ObjectId objId in _acad.GetLayerObjects(_collumnsLayer))
                            AddCollumn(tr, objId);
            }
        }

        public string EmptiesLayer
        {
            get { return _emptiesLayer; }
            set
            {
                _emptiesLayer = value;

                if (!string.IsNullOrEmpty(_emptiesLayer))
                    using (var tr = _acad.StartOpenCloseTransaction())
                        foreach (ObjectId objId in _acad.GetLayerObjects(_emptiesLayer))
                            AddEmpty(tr, objId);
            }
        }

        public ObjectId OutlineId
        {
            get { return _outlineId; }
            set
            {
                _outlineId = value;

                using (var tr = _acad.StartOpenCloseTransaction())
                    Outline = tr.GetObject(OutlineId, OpenMode.ForRead) as Polyline;
            }
        }

        public AcEnvironment(ObjectId outlineId)
        {
            _acad = new AutoCadManager();
            _girders = new List<Entity>();
            _collumns = new List<Entity>();
            _empties = new List<Polyline>();

            OutlineId = outlineId;
        }

        public void AddGirder(Transaction tr, ObjectId id)
        {
            var dbObject = tr.GetObject(id, OpenMode.ForRead);
            if (dbObject is Entity)
                _girders.Add(dbObject as Entity);
        }

        public void AddCollumn(Transaction tr, ObjectId id)
        {
            var dbObject = tr.GetObject(id, OpenMode.ForRead);
            if (dbObject is Entity)
                _collumns.Add(dbObject as Entity);
        }

        public void AddEmpty(Transaction tr, ObjectId id)
        {
            var emptyPolyline = tr.GetObject(id, OpenMode.ForRead) as Polyline;
            if (emptyPolyline != null && emptyPolyline.Closed)
                _empties.Add(emptyPolyline);
        }
    }
}
