using MediTrack.Frontend.Models.Model;
using MediTrack.Frontend.ViewModels.PantallasPrincipales;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Calendar;

namespace MediTrack.Frontend.Vistas.PantallasPrincipales
{
    public partial class PantallaAgenda : ContentPage
    {
        private AgendaViewModel _viewModel;

        public PantallaAgenda(AgendaViewModel viewModel)
        {
            try
            {
                InitializeComponent();

                // Crear ViewModel
                _viewModel = viewModel;
                BindingContext = _viewModel;

                //  CONFIGURACIÓN SIMPLE
                ConfigurarCalendario();
                SuscribirseAMensajes();

                System.Diagnostics.Debug.WriteLine("PantallaAgenda con Syncfusion inicializada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en constructor: {ex.Message}");
            }
        }

        private void ConfigurarCalendario()
        {
            try
            {
                // Solo configurar lo básico
                CalendarioSync.SelectionMode = CalendarSelectionMode.Single;

                System.Diagnostics.Debug.WriteLine(" Calendario configurado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configurando calendario: {ex.Message}");
            }
        }

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
                CalendarioSync.SelectedDate = nuevaFecha;
                CalendarioSync.DisplayDate = nuevaFecha;

                System.Diagnostics.Debug.WriteLine($"Navegado a mes anterior: {nuevaFecha:yyyy-MM}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navegando a mes anterior: {ex.Message}");
            }
        }

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
                CalendarioSync.SelectedDate = nuevaFecha;
                CalendarioSync.DisplayDate = nuevaFecha;

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
                var apiService = _viewModel.GetApiService();
                // Asegurar que el calendario esté en el mes correcto
                if (_viewModel != null)
                {
                    CalendarioSync.DisplayDate = _viewModel.FechaSeleccionada;
                    CalendarioSync.SelectedDate = _viewModel.FechaSeleccionada;
                    //_viewModel.CargarEventosDelDia();
                }

                System.Diagnostics.Debug.WriteLine("PantallaAgenda OnAppearing");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
            }
        }

        private void SuscribirseAMensajes()
        {
            // Mensaje para abrir modal de agregar evento
            MessagingCenter.Subscribe<AgendaViewModel>(this, "AbrirModalAgregarEvento", async (sender) =>
            {
                await AbrirModalAgregarEvento();
            });

            // Mensaje para abrir modal de editar evento
            MessagingCenter.Subscribe<AgendaViewModel, EventoMedicoUsuario>(this, "AbrirModalEditarEvento", async (sender, evento) =>
            {
                await AbrirModalEditarEvento(evento);
            });
        }

        private async Task AbrirModalAgregarEvento()
        {
            try
            {
                var apiService = _viewModel.GetApiService();
                var modal = new ModalAgregarEvento(_viewModel.FechaSeleccionada, apiService);

                await Navigation.PushModalAsync(modal);

                // Opcional: Recargar eventos después de cerrar el modal
                //await _viewModel.CargarEventosDelDia();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error abriendo modal: {ex.Message}");
                await DisplayAlert("Error", "No se pudo abrir el formulario", "OK");
            }
        }

        private async Task AbrirModalEditarEvento(EventoMedicoUsuario evento)
        {
            try
            {
                var apiService = _viewModel.GetApiService();
                var modal = new ModalAgregarEvento(evento, apiService);

                await Navigation.PushModalAsync(modal);
                //await _viewModel.CargarEventosDelDia();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error editando evento: {ex.Message}");
                await DisplayAlert("Error", "No se pudo abrir el editor", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<AgendaViewModel>(this, "AbrirModalAgregarEvento");
            MessagingCenter.Unsubscribe<AgendaViewModel, EventoMedicoUsuario>(this, "AbrirModalEditarEvento");
        }

    }
}