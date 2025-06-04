using System.ComponentModel;

namespace MediTrack.Frontend.Models
{
    public class EventoAgenda : INotifyPropertyChanged
    {
        private string _titulo = "";
        private string _descripcion = "";
        private DateTime _fechaHora;
        private string _tipo = "";
        private bool _completado;
        private string _color = "#007AFF";

        public string Titulo
        {
            get => _titulo;
            set
            {
                if (_titulo != value)
                {
                    _titulo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Titulo)));
                }
            }
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                if (_descripcion != value)
                {
                    _descripcion = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Descripcion)));
                }
            }
        }

        public DateTime FechaHora
        {
            get => _fechaHora;
            set
            {
                if (_fechaHora != value)
                {
                    _fechaHora = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FechaHora)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HoraFormateada)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HoraCompleta)));
                }
            }
        }

        public string Tipo
        {
            get => _tipo;
            set
            {
                if (_tipo != value)
                {
                    _tipo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tipo)));
                }
            }
        }

        public bool Completado
        {
            get => _completado;
            set
            {
                if (_completado != value)
                {
                    _completado = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Completado)));
                }
            }
        }

        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
                }
            }
        }

        // Propiedades calculadas para mejor presentación
        public string HoraFormateada => FechaHora.ToString("h:mm tt", new System.Globalization.CultureInfo("es-ES"));
        public string HoraCompleta => FechaHora.ToString("HH:mm");
        public string ColorMedicamento => Tipo == "Medicamento" ? "#34C759" : "#007AFF";

        public event PropertyChangedEventHandler PropertyChanged;
    }
}