using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using System.Globalization;
using MediTrack.Frontend.Services.Implementaciones;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
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

        private readonly EventosService _eventosService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        public AgendaViewModel()
        {
            try
            {
                // Usar servicio compartido
                _eventosService = EventosService.Instance;

                // Suscribirse a cambios
                _eventosService.EventoActualizado += OnEventoActualizado;

                // ✅ CONFIGURAR CULTURA ESPAÑOLA
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;

                ActualizarTextosFecha();
                CargarEventosDelDia();

                PropertyChanged += OnPropertyChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AgendaViewModel: {ex.Message}");
            }
        }

        private void OnEventoActualizado(object sender, EventoAgenda evento)
        {
            // Recargar eventos si el cambio afecta la fecha actual
            if (evento.FechaHora.Date == FechaSeleccionada.Date)
            {
                CargarEventosDelDia();
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

        public void CargarEventosDelDia()
        {
            try
            {
                EventosDelDia.Clear();

                // Usar servicio compartido
                var eventos = _eventosService.ObtenerEventos(FechaSeleccionada);

                foreach (var evento in eventos)
                {
                    EventosDelDia.Add(evento);
                }

                System.Diagnostics.Debug.WriteLine($"Cargados {EventosDelDia.Count} eventos para {FechaSeleccionada:yyyy-MM-dd} desde servicio");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando eventos: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AgregarEvento()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Abriendo modal para agregar evento en fecha: {FechaSeleccionada:yyyy-MM-dd}");

                // Crear y abrir el modal
                var modal = new ModalAgregarEvento(FechaSeleccionada);

                // Mostrar el modal
                await Application.Current.MainPage.Navigation.PushModalAsync(modal);

                // Esperar a que el modal se cierre
                await EsperarCierreModal(modal);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error abriendo modal: {ex.Message}");
            }
        }

        private async Task EsperarCierreModal(ModalAgregarEvento modal)
        {
            try
            {
                // Esperar en un bucle hasta que el modal se cierre
                while (Application.Current.MainPage.Navigation.ModalStack.Contains(modal))
                {
                    await Task.Delay(100);
                }

                // Si se guardó un evento, agregarlo al servicio
                if (modal.EventoGuardado && modal.EventoCreado != null)
                {
                    await AgregarEventoCreado(modal.EventoCreado);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error esperando cierre de modal: {ex.Message}");
            }
        }

        private async Task AgregarEventoCreado(EventoAgenda nuevoEvento)
        {
            try
            {
                // Agregar al servicio compartido
                _eventosService.AgregarEvento(nuevoEvento);

                // Los eventos se recargarán automáticamente por la notificación
                System.Diagnostics.Debug.WriteLine($"Evento agregado exitosamente al servicio: {nuevoEvento.Titulo} - {nuevoEvento.FechaHora:yyyy-MM-dd HH:mm}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando evento al servicio: {ex.Message}");
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

                    // Notificar al servicio que hubo cambios
                    _eventosService.ActualizarEstadoEvento(evento);

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

        // Cleanup
        ~AgendaViewModel()
        {
            if (_eventosService != null)
            {
                _eventosService.EventoActualizado -= OnEventoActualizado;
            }
        }
    }
}