using ReactiveUI;
using System;
using System.Collections;
using System.Xml.Serialization;
using Urbbox.SlabAssembler.Core.Variations;
using Urbbox.SlabAssembler.ViewModels;

namespace Urbbox.SlabAssembler.Core
{
    [Serializable]
    public class Part : ReactiveObject
    {
        private string _referenceName;
        public string ReferenceName {
            get { return _referenceName; }
            set { this.RaiseAndSetIfChanged(ref _referenceName, value); }
        }

        private float _width;
        public float Width {
            get { return _width; }
            set { this.RaiseAndSetIfChanged(ref _width, value); }
        }

        private float _height;
        public float Height {
            get { return _height; }
            set { this.RaiseAndSetIfChanged(ref _height, value); }
        }

        private UsageType _usageType;
        public UsageType UsageType {
            get { return _usageType; }
            set { this.RaiseAndSetIfChanged(ref _usageType, value); }
        }

        private string _layer;
        public string Layer {
            get { return _layer; }
            set { this.RaiseAndSetIfChanged(ref _layer, value); }
        }

        private string _name;
        [XmlAttribute]
        public string Name {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        private int _modulation;
        [XmlAttribute]
        public int Modulation {
            get { return _modulation; }
            set { this.RaiseAndSetIfChanged(ref _modulation, value); }
        }

        [XmlIgnore]
        public int Id => ReferenceName.GetHashCode();

        [XmlIgnore]
        public float GreatestDimension => (Width >= Height)? Width : Height;
        [XmlIgnore]
        public float SmallestDimension => (Width <= Height) ? Width : Height;

        public Part() { }

        public Part(string name, string referenceName)
        {
            Name = name;
            ReferenceName = referenceName;
        }

    }
}
