using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Implementaciones;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using MediTrack.Frontend.Vistas.PantallasPrincipales;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgendaViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string mesActual = "";

        [ObservableProperty]
        private string fechaFormateada = "";

        [ObservableProperty]
        private int eventosCompletados = 0;

        [ObservableProperty]
        private int eventosPendientes = 0;

        [ObservableProperty]
        private int totalEventos = 0;

        [ObservableProperty]
        private string estadisticasTexto = "";

        [ObservableProperty]
        private ObservableCollection<EventoMedicoUsuario> eventosDelDia = new();

        private readonly CultureInfo _culturaEspañola = new("es-ES");
        private readonly IApiService _apiService;
        private readonly int _idUsuario = 1; // TODO: Obtener dinámicamente desde autenticación

        public AgendaViewModel(IApiService apiService)
        {
                Title = "Agenda";
                CultureInfo.CurrentCulture = _culturaEspañola;
                CultureInfo.CurrentUICulture = _culturaEspañola;
                _apiService = apiService;
                ActualizarTextosFecha();
                PropertyChanged += OnPropertyChanged;
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                ActualizarTextosFecha();
                await CargarEventosDelDia();
            });
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
                MesActual = FechaSeleccionada.ToString("MMMM yyyy", _culturaEspañola);
                FechaFormateada = FechaSeleccionada.ToString("dddd, dd 'de' MMMM", _culturaEspañola);

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
                Debug.WriteLine($"Error actualizando textos de fecha: {ex.Message}");
            }
        }

        private async Task CargarEventosDelDia()
        {
            await ExecuteAsync(async () =>
            {
                try
                {
                    EventosDelDia.Clear();

                    var request = new ReqListarEventosUsuario
                    {
                        IdUsuario = _idUsuario,
                        FechaInicio = FechaSeleccionada.Date,
                        FechaFin = FechaSeleccionada.Date.AddDays(1).AddTicks(-1)
                    };

                    var response = await _apiService.ListarEventosUsuarioAsync(request);

                    if (response.resultado && response.Eventos != null)
                    {
                        foreach (var evento in response.Eventos)
                        {
                            evento.Completado = evento.EstadoEvento == "Completado";
                            EventosDelDia.Add(evento);
                        }
                        Debug.WriteLine($"Cargados {EventosDelDia.Count} eventos para {FechaSeleccionada:yyyy-MM-dd}");
                    }
                    else
                    {
                        await ShowAlertAsync("Error", response.errores?.FirstOrDefault()?.mensaje ?? "No se pudieron cargar los eventos.");
                    }

                    ActualizarEstadisticas();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cargando eventos: {ex.Message}");
                    await ShowAlertAsync("Error", "No se pudieron cargar los eventos.");
                }
            });
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                EventosCompletados = EventosDelDia.Count(e => e.Completado);
                EventosPendientes = EventosDelDia.Count(e => !e.Completado);
                TotalEventos = EventosDelDia.Count;

                if (TotalEventos == 0)
                {
                    EstadisticasTexto = "Sin eventos";
                }
                else
                {
                    var porcentaje = TotalEventos > 0 ? (EventosCompletados * 100 / TotalEventos) : 0;
                    EstadisticasTexto = $"{EventosCompletados} de {TotalEventos} completados ({porcentaje}%)";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando estadísticas: {ex.Message}");
                EstadisticasTexto = "Error en estadísticas";
            }
        }

        [RelayCommand]
        private async Task AgregarEvento()
        {
            try
            {
                MessagingCenter.Send(this, "AbrirModalAgregarEvento");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error abriendo modal agregar evento: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditarEvento(EventoMedicoUsuario evento)
        {
            try
            {
                MessagingCenter.Send(this, "AbrirModalEditarEvento", evento);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error abriendo modal editar evento: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EliminarEvento(EventoMedicoUsuario evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento == null) return;

                bool confirmar = await ShowConfirmAsync(
                    "Eliminar evento",
                    $"¿Estás seguro de que deseas eliminar '{evento.Titulo}'?",
                    "Eliminar", "Cancelar");

                if (!confirmar) return;

                // TODO: Implementar llamada al API para eliminar evento
                await ShowAlertAsync("En desarrollo", "La eliminación estará disponible pronto");
                // await CargarEventosDelDia(); // Recargar tras eliminar
            });
        }

        [RelayCommand]
        private async Task MarcarCompletado(EventoMedicoUsuario evento)
        {
            await ExecuteAsync(async () =>
            {
                if (evento == null) return;

                evento.Completado = !evento.Completado;
                evento.EstadoEvento = evento.Completado ? "Completado" : "Pendiente";

                // TODO: Implementar llamada al API para actualizar estado
                // await _apiService.ActualizarEstadoEventoAsync(evento.IdEventoMedico, evento.EstadoEvento);

                ActualizarEstadisticas();
                Debug.WriteLine($"Evento {evento.Titulo} marcado como {(evento.Completado ? "completado" : "pendiente")}");
            });
        }

        [RelayCommand]
        private async Task MostrarEstadisticas()
        {
            await ExecuteAsync(async () =>
            {
                if (TotalEventos == 0)
                {
                    await ShowAlertAsync("Estadísticas", "No hay eventos para este día");
                    return;
                }

                var porcentaje = TotalEventos > 0 ? (EventosCompletados * 100 / TotalEventos) : 0;
                var mensaje = $"{FechaFormateada}\n\n" +
                              $"Total de eventos: {TotalEventos}\n" +
                              $"Completados: {EventosCompletados}\n" +
                              $"Pendientes: {EventosPendientes}\n" +
                              $"Progreso: {porcentaje}%";

                await ShowAlertAsync("Estadísticas del día", mensaje);
            });
        }

        [RelayCommand]
        private async Task LimpiarCompletados()
        {
            await ExecuteAsync(async () =>
            {
                var eventosCompletados = EventosDelDia.Where(e => e.Completado).ToList();

                if (!eventosCompletados.Any())
                {
                    await ShowAlertAsync("Información", "No hay eventos completados para eliminar");
                    return;
                }

                bool confirmar = await ShowConfirmAsync(
                    "Limpiar completados",
                    $"¿Deseas eliminar los {eventosCompletados.Count} eventos completados?",
                    "Eliminar", "Cancelar");

                if (!confirmar) return;

                // TODO: Implementar llamada al API para eliminar eventos completados
                await ShowAlertAsync("En desarrollo", "La limpieza de completados estará disponible pronto");
                // await CargarEventosDelDia(); // Recargar tras eliminar
            });
        }

        [RelayCommand]
        private async Task NavegarMes(string direccion)
        {
            await ExecuteAsync(async () =>
            {
                FechaSeleccionada = direccion.ToLower() == "anterior"
                    ? FechaSeleccionada.AddMonths(-1)
                    : FechaSeleccionada.AddMonths(1);
            });
        }

        [RelayCommand]
        private async Task SeleccionarFecha(DateTime fecha)
        {
            await ExecuteAsync(async () =>
            {
                FechaSeleccionada = fecha;
            });
        }

        protected override async Task HandleErrorAsync(Exception exception)
        {
            await base.HandleErrorAsync(exception);
            ErrorMessage = exception.Message.Contains("servicio")
                ? "Error conectando con el servicio de eventos"
                : "Error inesperado en la agenda";
        }
    }
}