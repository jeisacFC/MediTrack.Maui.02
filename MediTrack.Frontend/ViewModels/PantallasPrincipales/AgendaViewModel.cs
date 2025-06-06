using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MediTrack.Frontend.Models;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using System.Globalization;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.ViewModels.Base;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgendaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private DateTime fechaSeleccionada = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<EventoAgenda> eventosDelDia = new();

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
                Title = "Agenda";

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

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                ActualizarTextosFecha();
                CargarEventosDelDia();
                await Task.CompletedTask;
            });
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
                // FORMATEAR FECHAS EN ESPAÑOL
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
            await ExecuteAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"Abriendo modal para agregar evento en fecha: {FechaSeleccionada:yyyy-MM-dd}");

                // Crear y abrir el modal con el nuevo ViewModel
                var modal = new ModalAgregarEvento(FechaSeleccionada);

                // Mostrar el modal
                await Application.Current.MainPage.Navigation.PushModalAsync(modal);

                // Esperar a que el modal se cierre
                await EsperarCierreModal(modal);
            });
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

                // Si se guardó un evento, ya está agregado automáticamente por el ViewModel del modal
                // Solo necesitamos confirmar que se guardó
                if (modal.EventoGuardado && modal.EventoCreado != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Evento guardado exitosamente: {modal.EventoCreado.Titulo}");

                    // Los eventos se recargarán automáticamente por la notificación del EventosService
                    // El AgregarEventoViewModel ya agregó el evento al servicio
                    // No necesitamos duplicar la lógica aquí
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error esperando cierre de modal: {ex.Message}");
                await HandleErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task MarcarCompletado(EventoAgenda evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento != null)
                {
                    evento.Completado = !evento.Completado;

                    // Notificar al servicio que hubo cambios
                    _eventosService.ActualizarEstadoEvento(evento);

                    System.Diagnostics.Debug.WriteLine($"Evento {evento.Titulo} marcado como {(evento.Completado ? "completado" : "pendiente")}");
                }
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task IrAHoy()
        {
            await ExecuteAsync(async () =>
            {
                FechaSeleccionada = DateTime.Today;
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task IrAMañana()
        {
            await ExecuteAsync(async () =>
            {
                FechaSeleccionada = DateTime.Today.AddDays(1);
                await Task.CompletedTask;
            });
        }

        [RelayCommand]
        private async Task Navegar(string destino)
        {
            await ExecuteAsync(async () =>
            {
                switch (destino.ToLower())
                {
                    case "hoy":
                        FechaSeleccionada = DateTime.Today;
                        break;
                    case "mañana":
                        FechaSeleccionada = DateTime.Today.AddDays(1);
                        break;
                    case "anterior":
                        FechaSeleccionada = FechaSeleccionada.AddDays(-1);
                        break;
                    case "siguiente":
                        FechaSeleccionada = FechaSeleccionada.AddDays(1);
                        break;
                }
                await Task.CompletedTask;
            });
        }

        // Método para recargar datos (puede ser llamado desde la UI)
        [RelayCommand]
        private async Task RecargarEventos()
        {
            await ExecuteAsync(async () =>
            {
                CargarEventosDelDia();
                await Task.CompletedTask;
            });
        }

        // Cleanup mejorado
        protected override async Task HandleErrorAsync(Exception exception)
        {
            await base.HandleErrorAsync(exception);

            // Manejo específico de errores de agenda
            if (exception.Message.Contains("servicio"))
            {
                ErrorMessage = "Error conectando con el servicio de eventos";
            }
            else
            {
                ErrorMessage = "Error inesperado en la agenda";
            }
        }

        // Destructor para limpiar eventos
        ~AgendaViewModel()
        {
            try
            {
                if (_eventosService != null)
                {
                    _eventosService.EventoActualizado -= OnEventoActualizado;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en destructor de AgendaViewModel: {ex.Message}");
            }
        }
    }
}