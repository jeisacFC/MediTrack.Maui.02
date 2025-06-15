using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Models.Response;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class EventosMedicosViewModel : BaseViewModel
    {
        #region Propiedades Observables

        // --- Propiedades para el calendario --- //
        [ObservableProperty] private DateTime _fechaSeleccionada = DateTime.Today;
        [ObservableProperty] private string _mesActual = DateTime.Today.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        [ObservableProperty] private string _fechaFormateada = DateTime.Today.ToString("dddd, dd 'de' MMMM", CultureInfo.CurrentCulture);

        // --- Colecciones de eventos --- //
        [ObservableProperty] private ObservableCollection<EventoMedicoUsuario> _eventosDelDia = new();
        [ObservableProperty] private ObservableCollection<EventoMedicoUsuario> _todosLosEventos = new();

        // --- Estadísticas --- //
        [ObservableProperty] private int _totalEventos = 0;
        [ObservableProperty] private int _eventosCompletados = 0;
        [ObservableProperty] private int _eventosPendientes = 0;
        [ObservableProperty] private string _estadisticasTexto = string.Empty;

        // --- Propiedades del usuario --- //
        [ObservableProperty] private int _idUsuario = 0;

        #endregion

        #region Comandos

        public IAsyncRelayCommand CargarEventosCommand { get; }
        public IAsyncRelayCommand RefrescarEventosCommand { get; }
        public IAsyncRelayCommand AgregarEventoCommand { get; }
        public IAsyncRelayCommand<EventoMedicoUsuario> EditarEventoCommand { get; }
        public IAsyncRelayCommand<EventoMedicoUsuario> EliminarEventoCommand { get; }
        public IAsyncRelayCommand<EventoMedicoUsuario> MarcarCompletadoCommand { get; }
        public IAsyncRelayCommand LimpiarCompletadosCommand { get; }
        public IAsyncRelayCommand MostrarEstadisticasCommand { get; }
        public IRelayCommand CambiarFechaCommand { get; }
        public IRelayCommand AnteriorMesCommand { get; }
        public IRelayCommand SiguienteMesCommand { get; }

        #endregion

        #region Eventos

        public event EventHandler<EventoMedicoUsuario> EventoEditarSolicitado;
        public event EventHandler<DateTime> EventoAgregarSolicitado;
        public event EventHandler EstadisticasSolicitadas;

        #endregion

        #region Dependencias

        private readonly IApiService _apiService;
        public IApiService GetApiService()
        {
            return _apiService;
        }
        #endregion

        #region Constructor

        public EventosMedicosViewModel(IApiService apiService) : base()
        {
            _apiService = apiService;
            Title = "Eventos Médicos";

            // Inicializar comandos
            CargarEventosCommand = new AsyncRelayCommand(EjecutarCargarEventos);
            RefrescarEventosCommand = new AsyncRelayCommand(EjecutarRefrescarEventos);
            AgregarEventoCommand = new AsyncRelayCommand(EjecutarAgregarEvento);
            EditarEventoCommand = new AsyncRelayCommand<EventoMedicoUsuario>(EjecutarEditarEvento);
            EliminarEventoCommand = new AsyncRelayCommand<EventoMedicoUsuario>(EjecutarEliminarEvento);
            MarcarCompletadoCommand = new AsyncRelayCommand<EventoMedicoUsuario>(EjecutarMarcarCompletado);
            LimpiarCompletadosCommand = new AsyncRelayCommand(EjecutarLimpiarCompletados);
            MostrarEstadisticasCommand = new AsyncRelayCommand(EjecutarMostrarEstadisticas);
            CambiarFechaCommand = new RelayCommand(EjecutarCambiarFecha);
            AnteriorMesCommand = new RelayCommand(EjecutarAnteriorMes);
            SiguienteMesCommand = new RelayCommand(EjecutarSiguienteMes);
        }

        #endregion

        #region Override Methods

        public override async Task InitializeAsync()
        {
            Debug.WriteLine("=== INICIANDO INITIALIZE ASYNC DE EVENTOS MÉDICOS ===");

            await ExecuteAsync(async () =>
            {
                await ObtenerIdUsuarioAsync();
                await EjecutarCargarEventosInterno();
                Debug.WriteLine("=== INITIALIZE ASYNC COMPLETADO ===");
            });
        }

        #endregion

        #region Métodos Privados de Inicialización

        private async Task ObtenerIdUsuarioAsync()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                Debug.WriteLine($"User ID obtenido del storage: {userIdStr}");

                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    Debug.WriteLine("ERROR: No se pudo obtener user_id del storage");
                    ErrorMessage = "No se pudo obtener la información del usuario";
                    await ShowAlertAsync("Error", "Sesión expirada. Por favor, inicia sesión nuevamente.");
                    await Shell.Current.GoToAsync("//Login");
                    return;
                }

                IdUsuario = userId;
                Debug.WriteLine($"Usuario establecido: ID = {IdUsuario}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener ID usuario: {ex.Message}");
                await ShowAlertAsync("Error", "Error al obtener información del usuario");
            }
        }

        #endregion

        #region Métodos de Comandos

        private async Task EjecutarCargarEventos()
        {
            await ExecuteAsync(async () =>
            {
                await EjecutarCargarEventosInterno();
            });
        }

        private async Task EjecutarCargarEventosInterno()
        {
            try
            {
                if (IdUsuario <= 0)
                {
                    await ObtenerIdUsuarioAsync();
                    if (IdUsuario <= 0) return;
                }

                // Usar ReqListarEventosUsuario en lugar de ReqObtenerEventosPorUsuario
                var request = new ReqListarEventosUsuario
                {
                    IdUsuario = IdUsuario
                };

                Debug.WriteLine($"=== CARGANDO EVENTOS PARA USUARIO {IdUsuario} ===");

                var response = await _apiService.ListarEventosUsuarioAsync(request);

                if (response != null && response.resultado)
                {
                    Debug.WriteLine($"Eventos cargados: {response.Eventos?.Count ?? 0}");

                    // Limpiar colección anterior
                    TodosLosEventos.Clear();

                    // Agregar nuevos eventos
                    if (response.Eventos != null)
                    {
                        foreach (var evento in response.Eventos)
                        {
                            TodosLosEventos.Add(evento);
                        }
                    }

                    // Filtrar eventos del día seleccionado
                    FiltrarEventosPorFecha();

                    Debug.WriteLine("=== EVENTOS CARGADOS EXITOSAMENTE ===");
                }
                else
                {
                    var errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                  response?.Mensaje ??
                                  "Error al cargar los eventos";

                    Debug.WriteLine($"Error al cargar eventos: {errorMsg}");
                    ErrorMessage = errorMsg;
                    await ShowAlertAsync("Error", errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción en CargarEventos: {ex.Message}");
                ErrorMessage = "Error de conexión al cargar eventos";
                await HandleErrorAsync(ex);
            }
        }

        private async Task EjecutarRefrescarEventos()
        {
            await ExecuteAsync(async () =>
            {
                await EjecutarCargarEventosInterno();
            });
        }

        private async Task EjecutarAgregarEvento()
        {
            await ExecuteAsync(async () =>
            {
                EventoAgregarSolicitado?.Invoke(this, FechaSeleccionada);
            });
        }

        private async Task EjecutarEditarEvento(EventoMedicoUsuario evento)
        {
            if (evento == null) return;

            await ExecuteAsync(async () =>
            {
                Debug.WriteLine($"Editando evento: {evento.Titulo} (ID: {evento.IdEventoMedico})");
                EventoEditarSolicitado?.Invoke(this, evento);
            });
        }

        private async Task EjecutarEliminarEvento(EventoMedicoUsuario evento)
        {
            if (evento == null) return;

            await ExecuteAsync(async () =>
            {
                bool confirmar = await ShowConfirmAsync(
                    "Confirmar eliminación",
                    $"¿Está seguro que desea eliminar el evento '{evento.Titulo}'?",
                    "Eliminar",
                    "Cancelar");

                if (!confirmar) return;

                // Usar ReqEliminarEventoMedico corregido (sin IdUsuario)
                var request = new ReqEliminarEventoMedico
                {
                    IdEventoMedico = evento.IdEventoMedico
                };

                Debug.WriteLine($"=== ELIMINANDO EVENTO ===");
                Debug.WriteLine($"ID Evento: {request.IdEventoMedico}");

                var response = await _apiService.EliminarEventoMedicoAsync(request);

                if (response != null && response.resultado)
                {
                    // Remover de las colecciones
                    TodosLosEventos.Remove(evento);
                    EventosDelDia.Remove(evento);

                    // Actualizar estadísticas
                    CalcularEstadisticas();

                    await ShowAlertAsync("Éxito", "Evento eliminado correctamente");
                    Debug.WriteLine("Evento eliminado exitosamente");
                }
                else
                {
                    var errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                  response?.Mensaje ??
                                  "Error al eliminar el evento";

                    Debug.WriteLine($"Error al eliminar evento: {errorMsg}");
                    ErrorMessage = errorMsg;
                    await ShowAlertAsync("Error", errorMsg);
                }
            });
        }

        private async Task EjecutarMarcarCompletado(EventoMedicoUsuario evento)
        {
            if (evento == null) return;

            await ExecuteAsync(async () =>
            {
                var nuevoEstado = evento.EstadoEvento == "Completado" ? "Pendiente" : "Completado";

                // Usar ReqActualizarEstadoEvento corregido (sin IdUsuario, sin NuevoEstado)
                var request = new ReqActualizarEstadoEvento
                {
                    IdEventoMedico = evento.IdEventoMedico,
                    EstadoEvento = nuevoEstado
                };

                Debug.WriteLine($"=== ACTUALIZANDO ESTADO EVENTO ===");
                Debug.WriteLine($"ID Evento: {request.IdEventoMedico}");
                Debug.WriteLine($"Nuevo Estado: {nuevoEstado}");

                var response = await _apiService.ActualizarEstadoEventoAsync(request);

                if (response != null && response.resultado)
                {
                    // Actualizar en memoria
                    evento.EstadoEvento = nuevoEstado;

                    // Notificar cambio en la UI
                    OnPropertyChanged(nameof(EventosDelDia));

                    // Actualizar estadísticas
                    CalcularEstadisticas();

                    var mensaje = nuevoEstado == "Completado" ? "Evento marcado como completado" : "Evento marcado como pendiente";
                    await ShowAlertAsync("Éxito", mensaje);

                    Debug.WriteLine($"Estado actualizado a: {nuevoEstado}");
                }
                else
                {
                    var errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                  response?.Mensaje ??
                                  "Error al actualizar el estado del evento";

                    Debug.WriteLine($"Error al actualizar estado: {errorMsg}");
                    ErrorMessage = errorMsg;
                    await ShowAlertAsync("Error", errorMsg);
                }
            });
        }

        private async Task EjecutarLimpiarCompletados()
        {
            if (EventosCompletados == 0) return;

            await ExecuteAsync(async () =>
            {
                bool confirmar = await ShowConfirmAsync(
                    "Limpiar completados",
                    $"¿Desea eliminar los {EventosCompletados} eventos completados de hoy?",
                    "Limpiar",
                    "Cancelar");

                if (!confirmar) return;

                var eventosAEliminar = EventosDelDia
                    .Where(e => e.EstadoEvento == "Completado")
                    .ToList();

                int eliminados = 0;
                foreach (var evento in eventosAEliminar)
                {
                    var request = new ReqEliminarEventoMedico
                    {
                        IdEventoMedico = evento.IdEventoMedico
                    };

                    var response = await _apiService.EliminarEventoMedicoAsync(request);

                    if (response != null && response.resultado)
                    {
                        TodosLosEventos.Remove(evento);
                        EventosDelDia.Remove(evento);
                        eliminados++;
                    }
                }

                if (eliminados > 0)
                {
                    CalcularEstadisticas();
                    await ShowAlertAsync("Éxito", $"{eliminados} eventos completados eliminados");
                }
                else
                {
                    await ShowAlertAsync("Error", "No se pudieron eliminar los eventos");
                }
            });
        }

        private async Task EjecutarMostrarEstadisticas()
        {
            await ExecuteAsync(async () =>
            {
                EstadisticasSolicitadas?.Invoke(this, EventArgs.Empty);
            });
        }

        private void EjecutarCambiarFecha()
        {
            FiltrarEventosPorFecha();
        }

        private void EjecutarAnteriorMes()
        {
            FechaSeleccionada = FechaSeleccionada.AddMonths(-1);
            ActualizarMesActual();
        }

        private void EjecutarSiguienteMes()
        {
            FechaSeleccionada = FechaSeleccionada.AddMonths(1);
            ActualizarMesActual();
        }

        #endregion

        #region Métodos Auxiliares

        private void FiltrarEventosPorFecha()
        {
            try
            {
                EventosDelDia.Clear();

                var eventosFecha = TodosLosEventos
                    .Where(e => e.FechaInicio.Date == FechaSeleccionada.Date)
                    .OrderBy(e => e.FechaInicio.TimeOfDay)
                    .ToList();

                Debug.WriteLine($"=== FILTRANDO EVENTOS ===");
                Debug.WriteLine($"Fecha seleccionada: {FechaSeleccionada:yyyy-MM-dd}");
                Debug.WriteLine($"Total eventos: {TodosLosEventos.Count}");
                Debug.WriteLine($"Eventos del día: {eventosFecha.Count}");

                foreach (var evento in eventosFecha)
                {
                    EventosDelDia.Add(evento);
                    Debug.WriteLine($"- {evento.Titulo} a las {evento.FechaInicio:HH:mm}");
                }

                // Actualizar estadísticas y fecha formateada
                CalcularEstadisticas();
                ActualizarFechaFormateada();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al filtrar eventos: {ex.Message}");
            }
        }

        private void CalcularEstadisticas()
        {
            TotalEventos = EventosDelDia.Count;
            EventosCompletados = EventosDelDia.Count(e => e.EstadoEvento == "Completado");
            EventosPendientes = TotalEventos - EventosCompletados;

            if (TotalEventos == 0)
            {
                EstadisticasTexto = "No hay eventos para este día";
            }
            else
            {
                EstadisticasTexto = $"{EventosCompletados} completados • {EventosPendientes} pendientes";
            }

            Debug.WriteLine($"Estadísticas: Total={TotalEventos}, Completados={EventosCompletados}, Pendientes={EventosPendientes}");
        }

        private void ActualizarMesActual()
        {
            MesActual = FechaSeleccionada.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        }

        private void ActualizarFechaFormateada()
        {
            FechaFormateada = FechaSeleccionada.ToString("dddd, dd 'de' MMMM", CultureInfo.CurrentCulture);
        }

        #endregion

        #region Métodos Públicos

        public void AgregarEventoLocal(EventoMedicoUsuario evento)
        {
            if (evento == null) return;

            TodosLosEventos.Add(evento);

            // Si el evento es del día seleccionado, agregarlo también a EventosDelDia
            if (evento.FechaInicio.Date == FechaSeleccionada.Date)
            {
                var posicion = 0;
                for (int i = 0; i < EventosDelDia.Count; i++)
                {
                    if (EventosDelDia[i].FechaInicio.TimeOfDay > evento.FechaInicio.TimeOfDay)
                        break;
                    posicion = i + 1;
                }

                EventosDelDia.Insert(posicion, evento);
                CalcularEstadisticas();
            }

            Debug.WriteLine($"Evento agregado localmente: {evento.Titulo}");
        }

        public void ActualizarEventoLocal(EventoMedicoUsuario eventoActualizado)
        {
            if (eventoActualizado == null) return;

            // Buscar y actualizar en TodosLosEventos
            var eventoExistente = TodosLosEventos.FirstOrDefault(e => e.IdEventoMedico == eventoActualizado.IdEventoMedico);
            if (eventoExistente != null)
            {
                var index = TodosLosEventos.IndexOf(eventoExistente);
                TodosLosEventos[index] = eventoActualizado;
            }

            // Refiltrar eventos del día
            FiltrarEventosPorFecha();

            Debug.WriteLine($"Evento actualizado localmente: {eventoActualizado.Titulo}");
        }

        public void EstablecerUsuario(int idUsuario)
        {
            IdUsuario = idUsuario;
            Debug.WriteLine($"Usuario establecido: ID = {idUsuario}");
        }

        public async Task RefrescarEventosAsync()
        {
            await EjecutarRefrescarEventos();
        }

        #endregion

        #region Métodos Parciales para NotifyCanExecuteChanged

        partial void OnFechaSeleccionadaChanged(DateTime value)
        {
            FiltrarEventosPorFecha();
            ActualizarMesActual();
            CambiarFechaCommand?.NotifyCanExecuteChanged();
        }


        #endregion
    }
}