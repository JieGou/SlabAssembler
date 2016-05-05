using ReactiveUI;
using Urbbox.SlabAssembler.Core;
using System.Xml.Serialization;

namespace Urbbox.SlabAssembler.Managers
{
    public class AssemblyOptions : ReactiveObject
    {
        private float _outlineDistance;
        public float OutlineDistance
        {
            get { return _outlineDistance; }
            set { this.RaiseAndSetIfChanged(ref _outlineDistance, value); }
        }

        private float _distanceBetweenLp;
        public float DistanceBetweenLp
        {
            get { return _distanceBetweenLp; }
            set { this.RaiseAndSetIfChanged(ref _distanceBetweenLp, value); }
        }

        private float _distanceBetweenLpAndLd;
        public float DistanceBetweenLpAndLd
        {
            get { return _distanceBetweenLpAndLd; }
            set { this.RaiseAndSetIfChanged(ref _distanceBetweenLpAndLd, value); }
        }

        private bool _useLds;
        public bool UseLds
        {
            get { return _useLds; }
            set { this.RaiseAndSetIfChanged(ref _useLds, value); }
        }

        private bool _useEndLp;
        public bool UseEndLp
        {
            get { return _useEndLp; }
            set { this.RaiseAndSetIfChanged(ref _useEndLp, value); }
        }

        private bool _useStartLp;
        public bool UseStartLp
        {
            get { return _useStartLp; }
            set { this.RaiseAndSetIfChanged(ref _useStartLp, value); }
        }

        public AssemblyOptions()
        {
            OutlineDistance = 0;
            DistanceBetweenLp = 0;
            DistanceBetweenLpAndLd = 0;
            UseLds = false;
            UseEndLp = false;
            UseStartLp = false;
        }
    }

    public class ConfigurationData : ReactiveObject
    {
        [XmlArray]
        public ReactiveList<Part> Parts { get; private set; }
        public AssemblyOptions Options { get; private set; }

        public ConfigurationData()
        {
            Parts = new ReactiveList<Part>();
            Options = new AssemblyOptions();
        }
    }
}
