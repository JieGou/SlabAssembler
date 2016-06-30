using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.Geometry;
using ReactiveUI;
using Urbbox.SlabAssembler.Core.Variations;
using System.Drawing;

namespace Urbbox.SlabAssembler.Core.Models
{
    public class Part : ReactiveObject
    {
        private float _width;
        private float _height;
        private UsageType _usageType;
        private string _layer;
        private float _pivotFixX;
        private float _pivotFixY;
        private string _referenceName;
        private string _name;
        private float _startOffset;
        private int _modulation;

        [XmlAttribute]
        public string Id { get; set; }

        public Point3d PivotPoint => new Point3d(PivotPointX, PivotPointY, 0);
        public string OutlineReferenceName => $"{ReferenceName}_OUT";
        public string GenericReferenceName => $"{ReferenceName}_GEN";

        public string ReferenceName
        {
            get { return _referenceName; }
            set { this.RaiseAndSetIfChanged(ref _referenceName, value); }
        }

        public float Width
        {
            get { return _width; }
            set { this.RaiseAndSetIfChanged(ref _width, value); }
        }


        public float Height {
            get { return _height; }
            set { this.RaiseAndSetIfChanged(ref _height, value); }
        }

        public UsageType UsageType {
            get { return _usageType; }
            set { this.RaiseAndSetIfChanged(ref _usageType, value); }
        }

        public string Layer {
            get { return _layer; }
            set { this.RaiseAndSetIfChanged(ref _layer, value); }
        }

        public float PivotPointX
        {
            get { return _pivotFixX; }
            set { this.RaiseAndSetIfChanged(ref _pivotFixX, value); }
        }

        public float PivotPointY
        {
            get { return _pivotFixY; }
            set { this.RaiseAndSetIfChanged(ref _pivotFixY, value); }
        }

        public float StartOffset
        {
            get { return _startOffset; }
            set { this.RaiseAndSetIfChanged(ref _startOffset, value); }
        }

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public int Modulation
        {
            get { return _modulation; }
            set { this.RaiseAndSetIfChanged(ref _modulation, value); }
        }

        public SizeF Dimensions => new SizeF(Width, Height);

        public ReactiveCommand<object> Save { get; }

        public Part()
        {
            Id = Guid.NewGuid().ToString();
            Width = 0;
            Height = 0;
            StartOffset = 0;
            PivotPointX = 0;
            PivotPointY = 0;
            Save = this.WhenAnyValue(x => x.ReferenceName, x => x.Name, x => x.Layer, x => x.Width, x => x.Height, x => x.Modulation)
                .Select(x => !string.IsNullOrEmpty(x.Item1) && !string.IsNullOrEmpty(x.Item2) && !string.IsNullOrEmpty(x.Item3) && x.Item4 > 0 && x.Item5 > 0 && x.Item6 > 0)
                .ToCommand();
        }

        public float GetOrientationAngle(float globalOrientation)
        {
            return UsageType.ToOrietationAngle(globalOrientation);
        }
    }
}
