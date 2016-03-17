using System.ComponentModel;
using System.Runtime.CompilerServices;
using Urbbox.SlabAssembler.Annotations;

namespace Urbbox.SlabAssembler.ViewModels
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
