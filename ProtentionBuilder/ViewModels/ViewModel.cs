using System.ComponentModel;
using System.Runtime.CompilerServices;
using Urbbox.AutoCAD.ProtentionBuilder.Annotations;

namespace Urbbox.AutoCAD.ProtentionBuilder.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
