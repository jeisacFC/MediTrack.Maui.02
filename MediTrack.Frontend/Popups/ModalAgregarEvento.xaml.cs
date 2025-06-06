using MediTrack.Frontend.Models;
using Microsoft.Maui.Controls;
using System.Globalization;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class ModalAgregarEvento : ContentPage
    {
        public EventoAgenda EventoCreado { get; private set; }
        public bool EventoGuardado { get; private set; } = false;

        private readonly DateTime _fechaSeleccionada;
        private readonly CultureInfo _culturaEspañola = new("es-ES");

        public ModalAgregarEvento(DateTime fechaSeleccionada)
        {
            InitializeComponent();

            _fechaSeleccionada = fechaSeleccionada;
            ConfigurarFecha();
            ConfigurarHoraInicial();
        }

        private void ConfigurarFecha()
        {
            try
            {
                // Formatear la fecha en español
                var fechaFormateada = _fechaSeleccionada.ToString("dddd, dd 'de' MMMM 'de' yyyy", _culturaEspañola);

                // Capitalizar primera letra
                if (!string.IsNullOrEmpty(fechaFormateada))
                {
                    fechaFormateada = char.ToUpper(fechaFormateada[0]) + fechaFormateada[1..];
                }

                LabelFecha.Text = fechaFormateada;

                System.Diagnostics.Debug.WriteLine($"Modal configurado para fecha: {fechaFormateada}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando fecha: {ex.Message}");
                LabelFecha.Text = _fechaSeleccionada.ToString("dd/MM/yyyy");
            }
        }

        private void ConfigurarHoraInicial()
        {
            try
            {
                // Configurar hora inicial basada en la hora actual
                var horaActual = DateTime.Now;

                // Si es la fecha de hoy, usar la hora actual + 1 hora
                if (_fechaSeleccionada.Date == DateTime.Today)
                {
                    var horaSugerida = horaActual.AddHours(1);
                    TimePickerHora.Time = new TimeSpan(horaSugerida.Hour, 0, 0);
                }
                else
                {
                    // Si es otra fecha, usar las 9:00 AM por defecto
                    TimePickerHora.Time = new TimeSpan(9, 0, 0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando hora inicial: {ex.Message}");
                TimePickerHora.Time = new TimeSpan(9, 0, 0);
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            try
            {
                // Animación del botón
                var boton = sender as Button;
                if (boton != null)
                {
                    await boton.ScaleTo(0.95, 100);
                    await boton.ScaleTo(1.0, 100);
                }

                // Cerrar modal sin guardar
                EventoGuardado = false;
                await Navigation.PopModalAsync();

                System.Diagnostics.Debug.WriteLine("Modal cancelado por el usuario");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cancelar: {ex.Message}");
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            try
            {
                // Animación del botón
                var boton = sender as Button;
                if (boton != null)
                {
                    await boton.ScaleTo(0.95, 100);
                    await boton.ScaleTo(1.0, 100);
                }

                // Validar datos
                if (string.IsNullOrWhiteSpace(EntryNombre.Text))
                {
                    await DisplayAlert("❌ Error", "Por favor ingresa un nombre para el evento", "OK");
                    return;
                }

                // Crear el evento
                await CrearEvento();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar: {ex.Message}");
                await DisplayAlert("❌ Error", "Ocurrió un error al guardar el evento", "OK");
            }
        }

        private async Task CrearEvento()
        {
            try
            {
                // Obtener datos del formulario
                var nombre = EntryNombre.Text?.Trim();
                var descripcion = EditorDescripcion.Text?.Trim();
                var hora = TimePickerHora.Time;
                var tipoSeleccionado = PickerTipo.SelectedItem?.ToString() ?? "Recordatorio";

                // Crear fecha y hora completa
                var fechaHoraCompleta = _fechaSeleccionada.Date.Add(hora);

                // Determinar color según el tipo
                var color = ObtenerColorPorTipo(tipoSeleccionado);

                // Crear el evento
                EventoCreado = new EventoAgenda
                {
                    Titulo = nombre,
                    Descripcion = string.IsNullOrWhiteSpace(descripcion) ? "" : descripcion,
                    FechaHora = fechaHoraCompleta,
                    Tipo = tipoSeleccionado,
                    Color = color,
                    Completado = false
                };

                EventoGuardado = true;

                // Mostrar confirmación
                await DisplayAlert("✅ Éxito", $"Evento '{nombre}' creado para las {hora:hh\\:mm}", "OK");

                // Cerrar modal
                await Navigation.PopModalAsync();

                System.Diagnostics.Debug.WriteLine($"Evento creado: {nombre} - {fechaHoraCompleta:yyyy-MM-dd HH:mm}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando evento: {ex.Message}");
                throw;
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

        private async void OnFondoTapped(object sender, EventArgs e)
        {
            try
            {
                // Mostrar confirmación antes de cerrar
                var resultado = await DisplayAlert("❓ Confirmar", "¿Deseas salir sin guardar?", "Sí", "No");
                if (resultado)
                {
                    EventoGuardado = false;
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cerrar desde fondo: {ex.Message}");
            }
        }

        // Método para manejar el botón de retroceso del dispositivo
        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var resultado = await DisplayAlert("❓ Confirmar", "¿Deseas salir sin guardar?", "Sí", "No");
                if (resultado)
                {
                    EventoGuardado = false;
                    await Navigation.PopModalAsync();
                }
            });

            return true; // Prevenir el comportamiento por defecto
        }
    }
}