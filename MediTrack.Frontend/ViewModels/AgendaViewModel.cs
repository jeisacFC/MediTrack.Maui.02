using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using System.Globalization;

namespace MediTrack.Frontend.ViewModels
{
    public partial class AgendaViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime fechaSeleccionada = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<EventoAgenda> eventosDelDia = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string mesActual = "";

        [ObservableProperty]
        private string fechaFormateada = "";

        private Dictionary<DateTime, List<EventoAgenda>> _eventosCache = new();
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        public AgendaViewModel()
        {
            try
            {
                // ✅ CONFIGURAR CULTURA ESPAÑOLA
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;
                
                ActualizarTextosFecha();
                InicializarEventos();
                CargarEventosDelDia();
                
                PropertyChanged += OnPropertyChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AgendaViewModel: {ex.Message}");
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FechaSeleccionada))
            {
                ActualizarTextosFecha();
                CargarEventosDelDia();
            }
        }

        private void ActualizarTextosFecha()
        {
            try
            {
                // ✅ FORMATEAR FECHAS EN ESPAÑOL
                MesActual = FechaSeleccionada.ToString("MMMM yyyy", _culturaEspañola);
                FechaFormateada = FechaSeleccionada.ToString("dddd, dd 'de' MMMM", _culturaEspañola);
                
                // Capitalizar primera letra
                if (!string.IsNullOrEmpty(MesActual))
                {
                    MesActual = char.ToUpper(MesActual[0]) + MesActual[1..];
                }
                
                if (!string.IsNullOrEmpty(FechaFormateada))
                {
                    FechaFormateada = char.ToUpper(FechaFormateada[0]) + FechaFormateada[1..];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando textos de fecha: {ex.Message}");
            }
        }

        private void InicializarEventos()
        {
            _eventosCache.Clear();

            // Eventos para HOY
            _eventosCache[DateTime.Today] = new List<EventoAgenda>
            {
                new EventoAgenda
                {
                    Titulo = "Omeprazol 20mg",
                    Descripcion = "En ayunas",
                    FechaHora = DateTime.Today.AddHours(7),
                    Tipo = "Medicamento",
                    Color = "#2196F3"
                },
                new EventoAgenda
                {
                    Titulo = "Paracetamol 500mg", 
                    Descripcion = "Con alimentos",
                    FechaHora = DateTime.Today.AddHours(8),
                    Tipo = "Medicamento",
                    Color = "#4CAF50"
                }
            };

            // Eventos para MAÑANA
            _eventosCache[DateTime.Today.AddDays(1)] = new List<EventoAgenda>
            {
                new EventoAgenda
                {
                    Titulo = "Cita médica",
                    Descripcion = "Dr. González - Chequeo general",
                    FechaHora = DateTime.Today.AddDays(1).AddHours(10),
                    Tipo = "Cita",
                    Color = "#FF5722"
                }
            };

            // Eventos para el día 15
            var fecha15 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 15);
            if (fecha15 >= DateTime.Today)
            {
                _eventosCache[fecha15] = new List<EventoAgenda>
                {
                    new EventoAgenda
                    {
                        Titulo = "Análisis de sangre",
                        Descripcion = "Laboratorio - En ayunas",
                        FechaHora = fecha15.AddHours(8),
                        Tipo = "Análisis",
                        Color = "#E91E63"
                    }
                };
            }
        }

        public void CargarEventosDelDia()
        {
            try
            {
                EventosDelDia.Clear();
                
                if (_eventosCache.TryGetValue(FechaSeleccionada.Date, out var eventos))
                {
                    foreach (var evento in eventos.OrderBy(e => e.FechaHora))
                    {
                        EventosDelDia.Add(evento);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Cargados {EventosDelDia.Count} eventos para {FechaSeleccionada:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando eventos: {ex.Message}");
            }
        }

        [RelayCommand]
        private void AgregarEvento()
        {
            try
            {
                var nuevoEvento = new EventoAgenda
                {
                    Titulo = $"Nuevo evento {DateTime.Now:HH:mm}",
                    Descripcion = "Evento agregado desde la app",
                    FechaHora = FechaSeleccionada.Date.AddHours(12),
                    Tipo = "Recordatorio",
                    Color = "#9C27B0"
                };

                if (!_eventosCache.ContainsKey(FechaSeleccionada.Date))
                {
                    _eventosCache[FechaSeleccionada.Date] = new List<EventoAgenda>();
                }
                
                _eventosCache[FechaSeleccionada.Date].Add(nuevoEvento);
                CargarEventosDelDia();
                
                System.Diagnostics.Debug.WriteLine($"Evento agregado: {nuevoEvento.Titulo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando evento: {ex.Message}");
            }
        }

        [RelayCommand]
        private void MarcarCompletado(EventoAgenda evento)
        {
            try
            {
                if (evento != null)
                {
                    evento.Completado = !evento.Completado;
                    System.Diagnostics.Debug.WriteLine($"Evento {evento.Titulo} marcado como {(evento.Completado ? "completado" : "pendiente")}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marcando completado: {ex.Message}");
            }
        }

        [RelayCommand]
        private void IrAHoy()
        {
            try
            {
                FechaSeleccionada = DateTime.Today;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error yendo a hoy: {ex.Message}");
            }
        }

        [RelayCommand]
        private void IrAMañana()
        {
            try
            {
                FechaSeleccionada = DateTime.Today.AddDays(1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error yendo a mañana: {ex.Message}");
            }
        }
    }
}