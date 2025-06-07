using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediTrack.Frontend.ViewModels.Base;
using MediTrack.Frontend.Services.Implementaciones;
using System.Collections.ObjectModel;
using System.Globalization;
using MediTrack.Frontend.Models.Model;

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

        public ObservableCollection<string> TiposEvento { get; } = new()
        {
            "Medicamento",
            "Cita médica",
            "Análisis",
            "Recordatorio",
            "Ejercicio",
            "Otro"
        };

        public EventoAgenda? EventoCreado { get; private set; }

        private readonly DateTime _fechaSeleccionada;
        private readonly EventosService _eventosService;
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        public AgregarEventoViewModel(DateTime fechaSeleccionada)
        {
            _fechaSeleccionada = fechaSeleccionada;
            _eventosService = EventosService.Instance;

            Title = "Nuevo Evento";
            ConfigurarFecha();
            ConfigurarHoraInicial();
        }

        public override async Task InitializeAsync()
        {
            await Task.CompletedTask;
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

                System.Diagnostics.Debug.WriteLine($"Modal configurado para fecha: {FechaFormateada}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando fecha: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Error configurando hora inicial: {ex.Message}");
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
                await CrearEvento();
            });
        }

        private async Task CrearEvento()
        {
            try
            {
                // Crear fecha y hora completa
                var fechaHoraCompleta = _fechaSeleccionada.Date.Add(HoraEvento);

                // Determinar color según el tipo
                var color = ObtenerColorPorTipo(TipoSeleccionado);

                // Crear el evento
                EventoCreado = new EventoAgenda
                {
                    Titulo = NombreEvento.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(DescripcionEvento) ? "" : DescripcionEvento.Trim(),
                    FechaHora = fechaHoraCompleta,
                    Tipo = TipoSeleccionado,
                    Color = color,
                    Completado = false
                };

                // Agregar al servicio y verificar resultado
                bool exitoso = _eventosService.AgregarEvento(EventoCreado);

                if (exitoso)
                {
                    EventoGuardado = true;

                    // Mostrar confirmación
                    await ShowAlertAsync("✅ Éxito", $"Evento '{NombreEvento}' creado para las {HoraEvento:hh\\:mm}");

                    // Cerrar modal
                    await CerrarModal();

                    System.Diagnostics.Debug.WriteLine($"Evento creado exitosamente: {NombreEvento} - {fechaHoraCompleta:yyyy-MM-dd HH:mm}");
                }
                else
                {
                    await ShowAlertAsync("❌ Error", "No se pudo guardar el evento. Inténtalo de nuevo.");
                    System.Diagnostics.Debug.WriteLine($"Error: No se pudo agregar el evento al servicio");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando evento: {ex.Message}");
                await ShowAlertAsync("❌ Error", "Ocurrió un error inesperado al guardar el evento");
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
                System.Diagnostics.Debug.WriteLine($"Error cerrando modal: {ex.Message}");
            }
        }

        private string ObtenerColorPorTipo(string tipo)
        {
            return tipo switch
            {
                "Medicamento" => "#2196F3",    // Azul
                "Cita médica" => "#FF5722",    // Rojo-naranja
                "Análisis" => "#E91E63",       // Rosa
                "Recordatorio" => "#9C27B0",   // Morado
                "Ejercicio" => "#4CAF50",      // Verde
                _ => "#607D8B"                 // Gris por defecto
            };
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