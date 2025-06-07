using System.ComponentModel;

namespace MediTrack.Frontend.Models.Model
{
    public class MedicamentoModel : INotifyPropertyChanged
    {
        private bool _tomado;

        public string Nombre { get; set; } = "";
        public string Hora { get; set; } = "";

        public bool Tomado
        {
            get => _tomado;
            set
            {
                if (_tomado != value)
                {
                    _tomado = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tomado)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}