using ReactiveUI;

namespace Urbbox.SlabAssembler.Core.Models
{
    public class AssemblyOptions : ReactiveObject
    {
        private float _outlineDistance;
        private float _distanceBetweenLp;
        private float _distanceBetweenLpAndLd;
        private bool _useLds;
        private bool _useEndLp;

        public float OutlineDistance
        {
            get { return _outlineDistance; }
            set { this.RaiseAndSetIfChanged(ref _outlineDistance, value); }
        }

        public float DistanceBetweenLp
        {
            get { return _distanceBetweenLp; }
            set { this.RaiseAndSetIfChanged(ref _distanceBetweenLp, value); }
        }

        public float DistanceBetweenLpAndLd
        {
            get { return _distanceBetweenLpAndLd; }
            set { this.RaiseAndSetIfChanged(ref _distanceBetweenLpAndLd, value); }
        }

        public bool UseLds
        {
            get { return _useLds; }
            set { this.RaiseAndSetIfChanged(ref _useLds, value); }
        }

        public bool UseEndLp
        {
            get { return _useEndLp; }
            set { this.RaiseAndSetIfChanged(ref _useEndLp, value); }
        }

        public AssemblyOptions()
        {
            OutlineDistance = 0;
            DistanceBetweenLp = 0;
            DistanceBetweenLpAndLd = 0;
            UseLds = false;
            UseEndLp = false;
        }
    }
}
