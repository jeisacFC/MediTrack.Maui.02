using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using MediTrack.Frontend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
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

                // Obtener ApiService desde DI o usar DependencyService como fallback
                var apiService = Handler?.MauiContext?.Services?.GetService<IApiService>()
                               ?? Microsoft.Maui.Controls.DependencyService.Get<IApiService>();

                // Crear ViewModel con dependencia
                _viewModel = new AgendaViewModel(apiService);
                BindingContext = _viewModel;

                //  CONFIGURACIÓN SIMPLE
                ConfigurarCalendario();

                System.Diagnostics.Debug.WriteLine("PantallaAgenda con Backend inicializada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en constructor de PantallaAgenda: {ex.Message}");
            }
        }

        //  MÉTODO SIMPLE Y LIMPIO
        private void ConfigurarCalendario()
        {
            try
            {
                // Configuración básica del calendario
                CalendarioSync.SelectionMode = CalendarSelectionMode.Single;
                CalendarioSync.SelectedDate = DateTime.Today;
                CalendarioSync.DisplayDate = DateTime.Today;

                System.Diagnostics.Debug.WriteLine(" Calendario configurado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error configurando calendario: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"❌ Error en selección de calendario: {ex.Message}");
            }
        }

        // Navegación anterior con animación
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

                // Actualizar ViewModel primero
                _viewModel.FechaSeleccionada = nuevaFecha;

                // Después actualizar Syncfusion
                CalendarioSync.SelectedDate = nuevaFecha;
                CalendarioSync.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes anterior: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error navegando a mes anterior: {ex.Message}");
            }
        }

        // Navegación siguiente con animación
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

                // Actualizar ViewModel primero
                _viewModel.FechaSeleccionada = nuevaFecha;

                // Después actualizar Syncfusion
                CalendarioSync.SelectedDate = nuevaFecha;
                CalendarioSync.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes siguiente: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error navegando a mes siguiente: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                // Asegurar que el calendario esté sincronizado
                if (_viewModel != null)
                {
                    CalendarioSync.DisplayDate = _viewModel.FechaSeleccionada;
                    CalendarioSync.SelectedDate = _viewModel.FechaSeleccionada;

                    // Inicializar ViewModel y cargar eventos
                    await _viewModel.InitializeAsync();
                }

                System.Diagnostics.Debug.WriteLine("PantallaAgenda OnAppearing - conectada al backend");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en OnAppearing: {ex.Message}");
            }
        }
    }
}