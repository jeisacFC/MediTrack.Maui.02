using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.Models.Request;
using MediTrack.Frontend.Services.Interfaces;
using MediTrack.Frontend.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgregarEventoViewModel : BaseViewModel
    {
        #region Properties Observables

        [ObservableProperty] private string _nombreEvento = string.Empty;
        [ObservableProperty] private string _descripcionEvento = string.Empty;
        [ObservableProperty] private TimeSpan _horaEvento = DateTime.Now.TimeOfDay;
        [ObservableProperty] private DateTime _fechaEvento;
        [ObservableProperty] private string _fechaFormateada = string.Empty;
        [ObservableProperty] private string _tipoSeleccionado = "Medicamento";
        [ObservableProperty] private ObservableCollection<string> _tiposEvento = new();
        [ObservableProperty] private bool _esEdicion = false;
        [ObservableProperty] private int _idEventoEditando = 0;

        // Propiedades para comunicación con la vista padre
        [ObservableProperty] private EventoMedicoUsuario? _eventoCreado;
        [ObservableProperty] private bool _eventoGuardado = false;

        #endregion

        #region Commands

        public IAsyncRelayCommand GuardarCommand { get; }
        public IAsyncRelayCommand CancelarCommand { get; }
        public IAsyncRelayCommand FondoTappedCommand { get; }

        #endregion

        #region Dependencies

        private readonly IApiService _apiService;
        public IApiService GetApiService()
        {
            return _apiService;
        }

        private int _idUsuario = 0;

        #endregion

        #region Constructor

        public AgregarEventoViewModel(DateTime fechaSeleccionada, IApiService apiService) : base()
        {
            _apiService = apiService;
            _fechaEvento = fechaSeleccionada;

            Title = "Agregar Evento";

            // Inicializar comandos
            GuardarCommand = new AsyncRelayCommand(EjecutarGuardar, PuedeGuardar);
            CancelarCommand = new AsyncRelayCommand(EjecutarCancelar);
            FondoTappedCommand = new AsyncRelayCommand(EjecutarCancelar);

            // Configurar tipos de evento
            ConfigurarTiposEvento();

            ActualizarFechaFormateada();
        }

        // Constructor para edición
        public AgregarEventoViewModel(EventoMedicoUsuario eventoExistente, IApiService apiService) : base()
        {
            _apiService = apiService;
            _esEdicion = true;
            _idEventoEditando = eventoExistente.IdEventoMedico;

            Title = "Editar Evento";

            // Cargar datos del evento existente
            _nombreEvento = eventoExistente.Titulo;
            _descripcionEvento = eventoExistente.Descripcion ?? string.Empty;
            _fechaEvento = eventoExistente.FechaInicio.Date;
            _horaEvento = eventoExistente.FechaInicio.TimeOfDay;
            _tipoSeleccionado = eventoExistente.TipoEvento ?? "Medicamento";

            // Inicializar comandos
            GuardarCommand = new AsyncRelayCommand(EjecutarGuardar, PuedeGuardar);
            CancelarCommand = new AsyncRelayCommand(EjecutarCancelar);
            FondoTappedCommand = new AsyncRelayCommand(EjecutarCancelar);

            // Configurar tipos de evento
            ConfigurarTiposEvento();

            ActualizarFechaFormateada();
        }

        #endregion

        #region Override Methods

        public override async Task InitializeAsync()
        {
            await ObtenerIdUsuarioAsync();
        }

        #endregion

        #region Private Methods

        private void ConfigurarTiposEvento()
        {
            TiposEvento.Clear();
            TiposEvento.Add("Medicamento");
            TiposEvento.Add("Cita médica");
            TiposEvento.Add("Ejercicio");
            TiposEvento.Add("Análisis");
            TiposEvento.Add("Otros");
        }

        private async Task ObtenerIdUsuarioAsync()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
                {
                    _idUsuario = userId;
                    Debug.WriteLine($"Usuario obtenido: {_idUsuario}");
                }
                else
                {
                    Debug.WriteLine("ERROR: No se pudo obtener user_id");
                    await ShowAlertAsync("Error", "No se pudo obtener información del usuario");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo ID usuario: {ex.Message}");
            }
        }

        private void ActualizarFechaFormateada()
        {
            FechaFormateada = FechaEvento.ToString("dddd, dd 'de' MMMM", CultureInfo.CurrentCulture);
        }

        private bool PuedeGuardar()
        {
            return !string.IsNullOrWhiteSpace(NombreEvento) && !IsBusy;
        }

        private async Task EjecutarGuardar()
        {
            await ExecuteAsync(async () =>
            {
                if (_idUsuario <= 0)
                {
                    await ShowAlertAsync("Error", "Usuario no válido");
                    return;
                }

                if (string.IsNullOrWhiteSpace(NombreEvento))
                {
                    await ShowAlertAsync("Error", "El nombre del evento es requerido");
                    return;
                }

                if (EsEdicion)
                {
                    await ActualizarEvento();
                }
                else
                {
                    await CrearEvento();
                }
            });
        }

        private async Task CrearEvento()
        {
            try
            {
                var fechaHoraCompleta = FechaEvento.Date.Add(HoraEvento);

                var request = new ReqInsertarEventoMedico
                {
                    IdUsuario = _idUsuario,
                    Titulo = NombreEvento.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(DescripcionEvento) ? null : DescripcionEvento.Trim(),
                    FechaInicio = fechaHoraCompleta,
                    FechaFin = fechaHoraCompleta.AddHours(1), // Duración por defecto de 1 hora
                    EsRecurrente = false,
                    IdTipoEvento = ObtenerIdTipoEvento(TipoSeleccionado),
                    EstadoEvento = "Pendiente"
                };

                Debug.WriteLine($"=== CREANDO EVENTO ===");
                Debug.WriteLine($"Usuario: {request.IdUsuario}");
                Debug.WriteLine($"Título: {request.Titulo}");
                Debug.WriteLine($"Fecha: {request.FechaInicio:yyyy-MM-dd HH:mm}");
                Debug.WriteLine($"Tipo: {request.IdTipoEvento}");

                var response = await _apiService.InsertarEventoMedicoAsync(request);

                if (response != null && response.resultado)
                {
                    // Crear el evento resultante para retornarlo
                    EventoCreado = new EventoMedicoUsuario
                    {
                        IdUsuario = _idUsuario,
                        Titulo = request.Titulo,
                        Descripcion = request.Descripcion,
                        FechaInicio = request.FechaInicio,
                        FechaFin = request.FechaFin,
                        EsRecurrente = request.EsRecurrente,
                        IdTipoEvento = request.IdTipoEvento,
                        EstadoEvento = request.EstadoEvento,
                        TipoEvento = TipoSeleccionado
                    };

                    EventoGuardado = true;
                    await ShowAlertAsync("Éxito", "Evento creado correctamente");
                    await CerrarModal();
                }
                else
                {
                    var errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                  response?.Mensaje ??
                                  "Error al crear el evento";

                    await ShowAlertAsync("Error", errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creando evento: {ex.Message}");
                await ShowAlertAsync("Error", "Error de conexión al crear el evento");
            }
        }

        private async Task ActualizarEvento()
        {
            try
            {
                var fechaHoraCompleta = FechaEvento.Date.Add(HoraEvento);

                var request = new ReqActualizarEventoMedico
                {
                    IdEventoMedico = IdEventoEditando,
                    Titulo = NombreEvento.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(DescripcionEvento) ? null : DescripcionEvento.Trim(),
                    FechaInicio = fechaHoraCompleta,
                    FechaFin = fechaHoraCompleta.AddHours(1),
                    IdTipoEvento = ObtenerIdTipoEvento(TipoSeleccionado)
                };

                Debug.WriteLine($"=== ACTUALIZANDO EVENTO ===");
                Debug.WriteLine($"ID: {request.IdEventoMedico}");
                Debug.WriteLine($"Título: {request.Titulo}");
                Debug.WriteLine($"Fecha: {request.FechaInicio:yyyy-MM-dd HH:mm}");

                var response = await _apiService.ActualizarEventoMedicoAsync(request);

                if (response != null && response.resultado)
                {
                    // Crear el evento actualizado para retornarlo
                    EventoCreado = new EventoMedicoUsuario
                    {
                        IdEventoMedico = IdEventoEditando,
                        IdUsuario = _idUsuario,
                        Titulo = request.Titulo,
                        Descripcion = request.Descripcion,
                        FechaInicio = request.FechaInicio,
                        FechaFin = request.FechaFin,
                        IdTipoEvento = request.IdTipoEvento,
                        TipoEvento = TipoSeleccionado,
                        EstadoEvento = "Pendiente" // Mantener estado existente
                    };

                    EventoGuardado = true;
                    await ShowAlertAsync("Éxito", "Evento actualizado correctamente");
                    await CerrarModal();
                }
                else
                {
                    var errorMsg = response?.errores?.FirstOrDefault()?.mensaje ??
                                  response?.Mensaje ??
                                  "Error al actualizar el evento";

                    await ShowAlertAsync("Error", errorMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando evento: {ex.Message}");
                await ShowAlertAsync("Error", "Error de conexión al actualizar el evento");
            }
        }

        private int ObtenerIdTipoEvento(string tipoEvento)
        {
            return tipoEvento switch
            {
                "Medicamento" => 2,
                "Cita médica" => 1,
                "Ejercicio" => 3,
                "Análisis" => 4,
                "Otros" => 5,
                _ => 1
            };
        }

        private async Task EjecutarCancelar()
        {
            await CerrarModal();
        }

        private async Task CerrarModal()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cerrando modal: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        public async Task<bool> OnBackButtonPressed()
        {
            await EjecutarCancelar();
            return true;
        }

        #endregion

        #region Property Changed Methods

        partial void OnNombreEventoChanged(string value)
        {
            GuardarCommand?.NotifyCanExecuteChanged();
        }

        partial void OnFechaEventoChanged(DateTime value)
        {
            ActualizarFechaFormateada();
        }

        #endregion
    }
}