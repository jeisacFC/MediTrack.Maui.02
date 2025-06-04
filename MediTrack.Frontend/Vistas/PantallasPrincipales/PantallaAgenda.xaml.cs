using MediTrack.Frontend.ViewModels;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Calendar;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class PantallaAgenda : ContentPage
    {
        private AgendaViewModel _viewModel;

        public PantallaAgenda()
        {
            try
            {
                InitializeComponent();

                // Crear ViewModel
                _viewModel = new AgendaViewModel();
                BindingContext = _viewModel;

                // ✅ CONFIGURACIÓN SIMPLE
                ConfigurarCalendario();

                System.Diagnostics.Debug.WriteLine("PantallaAgenda con Syncfusion inicializada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en constructor: {ex.Message}");
            }
        }

        // ✅ MÉTODO SIMPLE Y LIMPIO
        private void ConfigurarCalendario()
        {
            try
            {
                // Solo configurar lo básico
                CalendarioSyncfusion.SelectionMode = CalendarSelectionMode.Single;

                System.Diagnostics.Debug.WriteLine("✅ Calendario configurado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando calendario: {ex.Message}");
            }
        }

        // Manejar el evento de selección del calendario Syncfusion
        private void OnCalendarSelectionChanged(object sender, CalendarSelectionChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null && _viewModel != null)
                {
                    _viewModel.FechaSeleccionada = (DateTime)e.NewValue;
                    System.Diagnostics.Debug.WriteLine($"Fecha seleccionada: {e.NewValue:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en selección de calendario: {ex.Message}");
            }
        }

        // MEJORADO: Navegación anterior con animación
        private async void AnteriorMes(object sender, EventArgs e)
        {
            try
            {
                var fechaActual = _viewModel.FechaSeleccionada;
                var nuevaFecha = fechaActual.AddMonths(-1);

                // Animación visual del botón
                var boton = sender as Button;
                if (boton != null)
                {
                    await boton.ScaleTo(0.9, 100, Easing.CubicOut);
                    await boton.ScaleTo(1.0, 100, Easing.CubicOut);
                }

                // Actualizar PRIMERO el ViewModel
                _viewModel.FechaSeleccionada = nuevaFecha;

                // DESPUÉS actualizar Syncfusion
                CalendarioSyncfusion.SelectedDate = nuevaFecha;
                CalendarioSyncfusion.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes anterior: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a mes anterior: {ex.Message}");
            }
        }

        // MEJORADO: Navegación siguiente con animación
        private async void SiguienteMes(object sender, EventArgs e)
        {
            try
            {
                var fechaActual = _viewModel.FechaSeleccionada;
                var nuevaFecha = fechaActual.AddMonths(1);

                // Animación visual del botón
                var boton = sender as Button;
                if (boton != null)
                {
                    await boton.ScaleTo(0.9, 100, Easing.CubicOut);
                    await boton.ScaleTo(1.0, 100, Easing.CubicOut);
                }

                // Actualizar PRIMERO el ViewModel
                _viewModel.FechaSeleccionada = nuevaFecha;

                // DESPUÉS actualizar Syncfusion
                CalendarioSyncfusion.SelectedDate = nuevaFecha;
                CalendarioSyncfusion.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes siguiente: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a mes siguiente: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                // Asegurar que el calendario esté en el mes correcto
                if (_viewModel != null)
                {
                    CalendarioSyncfusion.DisplayDate = _viewModel.FechaSeleccionada;
                    CalendarioSyncfusion.SelectedDate = _viewModel.FechaSeleccionada;
                    _viewModel.CargarEventosDelDia();
                }

                System.Diagnostics.Debug.WriteLine("PantallaAgenda OnAppearing");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
            }
        }
    }
}