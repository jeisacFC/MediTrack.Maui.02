using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.ViewModels.Base;
using MediTrack.Frontend.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Globalization;
using MediTrack.Frontend.Models.Request;
using System.Diagnostics;

namespace MediTrack.Frontend.ViewModels.PantallasPrincipales
{
    public partial class AgregarEventoViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string nombreEvento = string.Empty;

        [ObservableProperty]
        private string descripcionEvento = string.Empty;

        [ObservableProperty]
        private TimeSpan horaEvento = new(9, 0, 0);

        [ObservableProperty]
        private string tipoSeleccionado = "Medicamento";

        [ObservableProperty]
        private string fechaFormateada = string.Empty;

        [ObservableProperty]
        private bool eventoGuardado = false;

        // Propiedad para compatibilidad con el modal
        public bool EventoCreado { get; private set; } = false;

        public ObservableCollection<string> TiposEvento { get; } = new()
        {
            "Medicamento",
            "Cita médica",
            "Análisis",
            "Recordatorio",
            "Ejercicio",
            "Otro"
        };

        private readonly DateTime _fechaSeleccionada;
        private readonly IApiService _apiService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");
        private int _idUsuarioActual = 0;

        public AgregarEventoViewModel(DateTime fechaSeleccionada, IApiService apiService)
        {
            _fechaSeleccionada = fechaSeleccionada;
            _apiService = apiService;

            Title = "Nuevo Evento";
            ConfigurarFecha();
            ConfigurarHoraInicial();
        }

        public override async Task InitializeAsync()
        {
            await ExecuteAsync(async () =>
            {
                await ObtenerIdUsuarioActual();
            });
        }

        private async Task ObtenerIdUsuarioActual()
        {
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var userId))
                {
                    _idUsuarioActual = userId;
                    Debug.WriteLine($"ID Usuario obtenido para nuevo evento: {_idUsuarioActual}");
                }
                else
                {
                    Debug.WriteLine("❌ No se pudo obtener ID del usuario desde SecureStorage");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error obteniendo ID usuario: {ex.Message}");
            }
        }

        private void ConfigurarFecha()
        {
            try
            {
                // Formatear la fecha en español
                var fechaFormateadaTemp = _fechaSeleccionada.ToString("dddd, dd 'de' MMMM 'de' yyyy", _culturaEspañola);

                // Capitalizar primera letra
                if (!string.IsNullOrEmpty(fechaFormateadaTemp))
                {
                    FechaFormateada = char.ToUpper(fechaFormateadaTemp[0]) + fechaFormateadaTemp[1..];
                }

                Debug.WriteLine($"Modal configurado para fecha: {FechaFormateada}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error configurando fecha: {ex.Message}");
                FechaFormateada = _fechaSeleccionada.ToString("dd/MM/yyyy");
            }
        }

        private void ConfigurarHoraInicial()
        {
            try
            {
                // Si es la fecha de hoy, usar la hora actual + 1 hora
                if (_fechaSeleccionada.Date == DateTime.Today)
                {
                    var horaSugerida = DateTime.Now.AddHours(1);
                    HoraEvento = new TimeSpan(horaSugerida.Hour, 0, 0);
                }
                else
                {
                    // Si es otra fecha, usar las 9:00 AM por defecto
                    HoraEvento = new TimeSpan(9, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error configurando hora inicial: {ex.Message}");
                HoraEvento = new TimeSpan(9, 0, 0);
            }
        }

        [RelayCommand]
        private async Task Guardar()
        {
            await ExecuteAsync(async () =>
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(NombreEvento))
                {
                    await ShowAlertAsync("❌ Error", "Por favor ingresa un nombre para el evento");
                    return;
                }

                if (NombreEvento.Trim().Length < 3)
                {
                    await ShowAlertAsync("❌ Error", "El nombre del evento debe tener al menos 3 caracteres");
                    return;
                }

                // Validar que tenemos usuario autenticado
                if (_idUsuarioActual <= 0)
                {
                    await ObtenerIdUsuarioActual();
                    if (_idUsuarioActual <= 0)
                    {
                        await ShowAlertAsync("❌ Error", "No se pudo obtener información del usuario. Intenta iniciar sesión nuevamente.");
                        return;
                    }
                }

                // Validar que la hora no sea en el pasado (solo para el día de hoy)
                if (_fechaSeleccionada.Date == DateTime.Today)
                {
                    var fechaHoraCompleta = _fechaSeleccionada.Date.Add(HoraEvento);
                    if (fechaHoraCompleta < DateTime.Now)
                    {
                        bool continuar = await ShowConfirmAsync(
                            "⚠️ Advertencia",
                            "La hora seleccionada ya pasó. ¿Deseas crear el evento de todas formas?",
                            "Sí", "No");

                        if (!continuar) return;
                    }
                }

                // Crear el evento
                await CrearEventoEnBackend();
            });
        }

        private async Task CrearEventoEnBackend()
        {
            try
            {
                Debug.WriteLine("Creando evento en el backend...");

                // Crear fecha y hora completa
                var fechaHoraCompleta = _fechaSeleccionada.Date.Add(HoraEvento);

                // Crear request para el backend
                var request = new ReqInsertarEventoMedico
                {
                    IdUsuario = _idUsuarioActual,
                    Titulo = NombreEvento.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(DescripcionEvento) ? "" : DescripcionEvento.Trim(),
                    FechaEvento = fechaHoraCompleta,
                    FechaRecordatorio = fechaHoraCompleta.AddMinutes(-15) // 15 minutos antes por defecto
                };

                Debug.WriteLine($"Enviando evento al backend:");
                Debug.WriteLine($"   - Usuario: {request.IdUsuario}");
                Debug.WriteLine($"   - Título: {request.Titulo}");
                Debug.WriteLine($"   - Fecha: {request.FechaEvento:yyyy-MM-dd HH:mm}");
                Debug.WriteLine($"   - Descripción: {request.Descripcion}");

                // Llamar al backend
                var response = await _apiService.InsertarEventoAsync(request);

                if (response != null && response.resultado)
                {
                    EventoGuardado = true;
                    EventoCreado = true; // Marcar como creado

                    // Mostrar confirmación
                    await ShowAlertAsync("✅ Éxito", $"Evento '{NombreEvento}' creado para las {HoraEvento:hh\\:mm}");

                    // Cerrar modal
                    await CerrarModal();

                    Debug.WriteLine($"Evento creado exitosamente en el backend: {NombreEvento} - {fechaHoraCompleta:yyyy-MM-dd HH:mm}");
                }
                else
                {
                    var errorMsg = response?.Mensaje ?? "Error desconocido";
                    var errores = response?.errores?.FirstOrDefault()?.mensaje ?? "";

                    Debug.WriteLine($"❌ Error del backend: {errorMsg} - {errores}");

                    await ShowAlertAsync("❌ Error", $"No se pudo guardar el evento: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error creando evento: {ex.Message}");
                await ShowAlertAsync("❌ Error", "Ocurrió un error de conexión al guardar el evento. Verifica tu conexión a internet.");
            }
        }

        [RelayCommand]
        private async Task Cancelar()
        {
            bool confirmar = await ShowConfirmAsync("❓ Confirmar", "¿Deseas salir sin guardar?", "Sí", "No");

            if (confirmar)
            {
                EventoGuardado = false;
                await CerrarModal();
            }
        }

        private async Task CerrarModal()
        {
            try
            {
                await Shell.Current.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error cerrando modal: {ex.Message}");
            }
        }

        // Comando para manejar el tap en el fondo (cerrar modal)
        [RelayCommand]
        private async Task FondoTapped()
        {
            await Cancelar();
        }

        // Método para ser llamado cuando se cierra con el botón de retroceso
        public async Task<bool> OnBackButtonPressed()
        {
            await Cancelar();
            return true; // Prevenir el comportamiento por defecto
        }
    }
}